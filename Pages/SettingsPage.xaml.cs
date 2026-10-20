using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var repo = Repositorio.Instancia;
        Ruta.Text = repo.Ruta;
        Resumen.Text = $"{repo.Equipos.Count} equipos · {repo.Mantenimientos.Count} órdenes · {repo.Calibraciones.Count} certificados.";
        Aviso.IsOpen = false;
    }

    private async void Restaurar_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new ContentDialog
        {
            Title = "Restaurar ejemplo",
            Content = "Se reemplazan los registros actuales por el ejemplo de la clínica.",
            PrimaryButtonText = "Restaurar",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        if (await dialogo.ShowAsync() != ContentDialogResult.Primary)
            return;

        Repositorio.Instancia.RestaurarEjemplo();
        Resumen.Text = $"{Repositorio.Instancia.Equipos.Count} equipos · {Repositorio.Instancia.Mantenimientos.Count} órdenes · {Repositorio.Instancia.Calibraciones.Count} certificados.";
        Aviso.IsOpen = true;
    }
}
