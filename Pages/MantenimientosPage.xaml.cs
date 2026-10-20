using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Dialogos;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Pages;

public sealed partial class MantenimientosPage : Page
{
    public ObservableCollection<FilaLista> Filas { get; } = [];

    private static readonly (string Texto, Func<EstadoOrden, bool> Coincide)[] FiltrosDisponibles =
    [
        ("Todas", _ => true),
        ("Pendientes", estado => estado != EstadoOrden.Completado),
        ("Vencidas", estado => estado == EstadoOrden.Vencido),
        ("En curso", estado => estado == EstadoOrden.EnCurso),
        ("Completadas", estado => estado == EstadoOrden.Completado)
    ];

    private bool _listo;
    private bool _cargando;
    private string? _pendiente;
    private string? _seleccionadoId;

    public MantenimientosPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.New && e.Parameter is string id)
            _pendiente = id;

        if (!_listo)
        {
            _cargando = true;
            foreach (var filtro in FiltrosDisponibles)
                Filtros.Items.Add(filtro.Texto);
            Filtros.SelectedIndex = 0;
            _cargando = false;
            _listo = true;
        }

        if (!ResolverEquipoPendiente())
            return;

        Repositorio.Instancia.Cambio += AplicarFiltro;
        AplicarFiltro();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Repositorio.Instancia.Cambio -= AplicarFiltro;
    }

    private bool ResolverEquipoPendiente()
    {
        const string prefijo = "equipo:";
        if (_pendiente is null || !_pendiente.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
            return true;

        var equipoId = _pendiente[prefijo.Length..];
        var repo = Repositorio.Instancia;
        var orden = repo.Mantenimientos
            .Where(m => string.Equals(m.EquipoId, equipoId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => repo.EstadoEfectivo(m) == EstadoOrden.Completado)
            .ThenByDescending(m => m.FechaProgramada)
            .FirstOrDefault();
        if (orden is null)
        {
            _pendiente = null;
            MainWindow.Instancia?.Navegar("equipos", equipoId);
            return false;
        }

        _cargando = true;
        Filtros.SelectedIndex = 0;
        _cargando = false;
        _pendiente = orden.Id;
        return true;
    }

    private void Filtros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_cargando && _listo)
            AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var repo = Repositorio.Instancia;
        var indice = Filtros.SelectedIndex < 0 ? 0 : Filtros.SelectedIndex;
        var coincide = FiltrosDisponibles[indice].Coincide;
        var previo = _pendiente ?? _seleccionadoId;
        _pendiente = null;

        var ordenes = repo.Mantenimientos
            .Where(m => coincide(repo.EstadoEfectivo(m)))
            .OrderBy(m => repo.EstadoEfectivo(m) == EstadoOrden.Completado)
            .ThenBy(m => m.FechaProgramada);

        Filas.Clear();
        foreach (var orden in ordenes)
            Filas.Add(CrearFila(repo, orden));

        Vacio.Visibility = Filas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        Lista.SelectedItem = Filas.FirstOrDefault(f => f.Id == previo) ?? Filas.FirstOrDefault();
    }

    private void Lista_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Lista.SelectedItem is not FilaLista fila)
        {
            _seleccionadoId = null;
            Detalle.Visibility = Visibility.Collapsed;
            DetalleVacio.Visibility = Visibility.Visible;
            return;
        }

        _seleccionadoId = fila.Id;
        var orden = Repositorio.Instancia.OrdenPorId(fila.Id);
        if (orden is not null)
            Mostrar(orden);
    }

    private void Mostrar(OrdenMantenimiento orden)
    {
        var repo = Repositorio.Instancia;
        var estado = repo.EstadoEfectivo(orden);
        var equipo = repo.EquipoPorId(orden.EquipoId);
        var cerrada = estado == EstadoOrden.Completado;

        DetalleVacio.Visibility = Visibility.Collapsed;
        Detalle.Visibility = Visibility.Visible;
        Codigo.Text = orden.Id;
        Titulo.Text = equipo?.Nombre ?? orden.EquipoId;
        EstadoLinea.Text = $"{Formato.Estado(estado)} · {Formato.Plazo(orden.FechaProgramada)}";
        TipoTexto.Text = Formato.Tipo(orden.Tipo);
        Tecnico.Text = orden.Tecnico;
        FechaProgramada.Text = Formato.Fecha(orden.FechaProgramada);
        FechaEjecucion.Text = orden.FechaEjecucion is DateTime hecha
            ? Formato.Fecha(hecha)
            : "Pendiente";
        Descripcion.Text = orden.Descripcion;
        Hallazgos.Text = orden.Hallazgos;
        Hallazgos.IsReadOnly = cerrada;
        BotonCerrar.Visibility = cerrada ? Visibility.Collapsed : Visibility.Visible;
        VerEquipo.Content = equipo is null ? "Ver ficha del equipo" : $"Ver ficha de {equipo.Nombre}";
    }

    private async void Nueva_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new DialogoMantenimiento { XamlRoot = XamlRoot };
        if (await dialogo.ShowAsync() == ContentDialogResult.Primary && dialogo.IdCreado is string id)
        {
            _pendiente = id;
            AplicarFiltro();
        }
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null)
            return;

        Repositorio.Instancia.CompletarMantenimiento(_seleccionadoId, Hallazgos.Text);
    }

    private void VerEquipo_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null)
            return;
        var equipoId = Repositorio.Instancia.OrdenPorId(_seleccionadoId)?.EquipoId;
        if (equipoId is not null)
            MainWindow.Instancia?.Navegar("equipos", equipoId);
    }

    private async void ExportarOrden_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null || Repositorio.Instancia.OrdenPorId(_seleccionadoId) is not OrdenMantenimiento orden)
        {
            await AvisarAsync("Selecciona una orden.");
            return;
        }

        await GuardarPdf.GuardarAsync(XamlRoot, orden.Id, ruta => ExportacionPdf.Orden(orden, ruta));
    }

    private async void ExportarLista_Click(object sender, RoutedEventArgs e)
    {
        var repo = Repositorio.Instancia;
        var indice = Filtros.SelectedIndex < 0 ? 0 : Filtros.SelectedIndex;
        var coincide = FiltrosDisponibles[indice].Coincide;
        var ordenes = repo.Mantenimientos.Where(m => coincide(repo.EstadoEfectivo(m))).ToList();
        if (ordenes.Count == 0)
        {
            await AvisarAsync("No hay órdenes para exportar.");
            return;
        }

        await GuardarPdf.GuardarAsync(XamlRoot, "Mantenimiento", ruta => ExportacionPdf.Ordenes(ordenes, ruta));
    }

    private async Task AvisarAsync(string mensaje)
    {
        var dialogo = new ContentDialog
        {
            Title = "Exportar PDF",
            Content = mensaje,
            CloseButtonText = "Cerrar",
            XamlRoot = XamlRoot
        };
        await dialogo.ShowAsync();
    }

    private static FilaLista CrearFila(Repositorio repo, OrdenMantenimiento orden)
    {
        var estado = repo.EstadoEfectivo(orden);
        var equipo = repo.EquipoPorId(orden.EquipoId);
        return new FilaLista
        {
            Id = orden.Id,
            Titulo = equipo?.Nombre ?? orden.EquipoId,
            Detalle = $"{orden.Id} · {Formato.Tipo(orden.Tipo)} · {Formato.FechaCorta(orden.FechaProgramada)}",
            Estado = Formato.Estado(estado),
            ColorEstado = Apariencia.De(estado)
        };
    }
}
