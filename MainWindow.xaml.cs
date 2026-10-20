using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Pages;
using MantenimientoCapilar.Servicios;
using Windows.Graphics;

namespace MantenimientoCapilar;

public sealed partial class MainWindow : Window
{
    public static MainWindow? Instancia { get; private set; }

    private bool _sincronizando;
    private object? _parametro;

    public MainWindow()
    {
        Instancia = this;
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        Iderma.Shell.BotonLauncher.Colocar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppTitleBar.Loaded += (_, _) => SincronizarAltoTitulo();
        AppWindow.Changed += (_, _) => SincronizarAltoTitulo();
        var icono = Path.Combine(AppContext.BaseDirectory, "img", "mantenimiento.ico");
        if (File.Exists(icono))
            AppWindow.SetIcon(icono);
        AppWindow.Resize(new SizeInt32(1360, 900));

        Repositorio.Instancia.Cambio += ActualizarInsignias;
        ActualizarInsignias();
        AperturaEquipo.Pedido += AbrirDesdeLogistica;
    }

    public void Navegar(string etiqueta, object? parametro = null)
    {
        _parametro = parametro;
        if (etiqueta == "ajustes")
        {
            if (ReferenceEquals(NavView.SelectedItem, NavView.SettingsItem))
                Abrir("ajustes");
            else
                NavView.SelectedItem = NavView.SettingsItem;
            return;
        }

        var item = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => Equals(i.Tag, etiqueta));
        if (item is null)
            return;

        if (ReferenceEquals(NavView.SelectedItem, item))
            Abrir(etiqueta);
        else
            NavView.SelectedItem = item;
    }

    private void SincronizarAltoTitulo()
    {
        if (AppTitleBar.XamlRoot is null)
            return;

        var escala = AppTitleBar.XamlRoot.RasterizationScale;
        if (escala <= 0)
            escala = 1;

        var alto = AppWindow.TitleBar.Height / escala;
        if (alto < 48)
            alto = 48;

        FilaTitulo.Height = new GridLength(alto);
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        if (NavFrame.CanGoBack)
            NavFrame.GoBack();
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        if (NavView.SettingsItem is NavigationViewItem ajustes)
            ajustes.Content = "Ajustes";

        var equipo = AperturaEquipo.Consumir();
        if (!string.IsNullOrWhiteSpace(equipo))
            Navegar("mantenimiento", "equipo:" + equipo);
    }

    private void AbrirDesdeLogistica(string equipoId)
    {
        if (string.IsNullOrWhiteSpace(equipoId))
            return;

        DispatcherQueue.TryEnqueue(() =>
        {
            AperturaEquipo.Consumir();
            Activate();
            Navegar("mantenimiento", "equipo:" + equipoId);
        });
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_sincronizando)
            return;

        if (args.IsSettingsSelected)
            Abrir("ajustes");
        else if (args.SelectedItem is NavigationViewItem item && item.Tag is string etiqueta)
            Abrir(etiqueta);
    }

    private void NavFrame_Navigated(object sender, NavigationEventArgs e)
    {
        var etiqueta = e.SourcePageType.Name switch
        {
            nameof(InicioPage) => "inicio",
            nameof(EquiposPage) => "equipos",
            nameof(MantenimientosPage) => "mantenimiento",
            nameof(CalibracionesPage) => "calibracion",
            nameof(SettingsPage) => "ajustes",
            _ => null
        };

        if (etiqueta is null)
            return;

        _sincronizando = true;
        if (etiqueta == "ajustes")
            NavView.SelectedItem = NavView.SettingsItem;
        else
            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => Equals(i.Tag, etiqueta));
        _sincronizando = false;
    }

    private void Abrir(string etiqueta)
    {
        var parametro = _parametro;
        _parametro = null;
        var pagina = etiqueta switch
        {
            "equipos" => typeof(EquiposPage),
            "mantenimiento" => typeof(MantenimientosPage),
            "calibracion" => typeof(CalibracionesPage),
            "ajustes" => typeof(SettingsPage),
            _ => typeof(InicioPage)
        };

        NavFrame.Navigate(pagina, parametro);
    }

    private void ActualizarInsignias()
    {
        var repo = Repositorio.Instancia;
        Poner(BadgeMantenimiento, repo.ContarMantenimientosVencidos(), "CriticalValueInfoBadgeStyle");
        Poner(BadgeCalibracion, repo.ContarCalibracionesEnAlerta(), "CautionValueInfoBadgeStyle");
    }

    private static void Poner(InfoBadge badge, int valor, string estilo)
    {
        badge.Value = valor;
        badge.Visibility = valor > 0 ? Visibility.Visible : Visibility.Collapsed;
        if (Application.Current.Resources.TryGetValue(estilo, out var recurso) && recurso is Style estiloBadge)
            badge.Style = estiloBadge;
    }
}
