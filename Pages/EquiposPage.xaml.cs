using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Pages;

public sealed partial class EquiposPage : Page
{
    public ObservableCollection<FilaLista> Filas { get; } = [];

    private bool _listo;
    private bool _cargando;
    private string? _pendiente;
    private string? _seleccionadoId;

    public EquiposPage()
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
            Categorias.Items.Add(new OpcionCategoria("Todas las categorías", null));
            foreach (var categoria in Enum.GetValues<CategoriaEquipo>())
                Categorias.Items.Add(new OpcionCategoria(Formato.Categoria(categoria), categoria));
            Categorias.SelectedIndex = 0;
            _cargando = false;
            _listo = true;
        }

        Repositorio.Instancia.Cambio += AplicarFiltro;
        AplicarFiltro();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Repositorio.Instancia.Cambio -= AplicarFiltro;
    }

    private void Buscar_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && !_cargando)
            AplicarFiltro();
    }

    private void Categorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_cargando && _listo)
            AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var repo = Repositorio.Instancia;
        var texto = Buscar.Text.Trim();
        var categoria = (Categorias.SelectedItem as OpcionCategoria)?.Categoria;
        var previo = _pendiente ?? _seleccionadoId;
        _pendiente = null;

        var equipos = repo.Equipos.Where(eq =>
        {
            if (categoria is not null && eq.Categoria != categoria)
                return false;
            if (texto.Length == 0)
                return true;
            return Contiene(eq.Nombre, texto)
                || Contiene(eq.Id, texto)
                || Contiene(eq.NumeroSerie, texto)
                || Contiene(eq.Ubicacion, texto)
                || Contiene(eq.Marca, texto);
        }).OrderBy(eq => eq.Id);

        Filas.Clear();
        foreach (var equipo in equipos)
            Filas.Add(CrearFila(repo, equipo));

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
        var equipo = Repositorio.Instancia.EquipoPorId(fila.Id);
        if (equipo is null)
            return;

        Mostrar(equipo);
    }

    private void Mostrar(Equipo equipo)
    {
        var repo = Repositorio.Instancia;
        DetalleVacio.Visibility = Visibility.Collapsed;
        Detalle.Visibility = Visibility.Visible;
        Codigo.Text = equipo.Id;
        Titulo.Text = equipo.Nombre;
        EstadoTexto.Text = Formato.Estado(equipo.Estado);
        EstadoPunto.Fill = Apariencia.De(equipo.Estado);
        MarcaModelo.Text = $"{equipo.Marca} · {equipo.Modelo}";
        Serie.Text = equipo.NumeroSerie;
        CategoriaTexto.Text = Formato.Categoria(equipo.Categoria);
        Ubicacion.Text = equipo.Ubicacion;
        Responsable.Text = equipo.Responsable;
        Intervalos.Text = equipo.RequiereCalibracion
            ? $"Mantenimiento cada {equipo.IntervaloMantenimientoDias} días · calibración cada {equipo.IntervaloCalibracionDias} días"
            : $"Mantenimiento cada {equipo.IntervaloMantenimientoDias} días · sin calibración periódica";
        Notas.Text = equipo.Notas;

        var abierta = repo.Mantenimientos
            .Where(m => m.EquipoId == equipo.Id && repo.EstadoEfectivo(m) != EstadoOrden.Completado)
            .OrderBy(m => m.FechaProgramada)
            .FirstOrDefault();
        ProximaOrden.Text = abierta is null
            ? "Sin orden abierta."
            : $"{abierta.Id} · {Formato.Tipo(abierta.Tipo)} · {Formato.FechaCorta(abierta.FechaProgramada)} · {Formato.Estado(repo.EstadoEfectivo(abierta))}";

        EstadoCalibracion.Text = TextoCalibracion(repo, equipo);
        var ultima = repo.UltimaCalibracion(equipo.Id);
        AbrirCertificado.IsEnabled = ultima is not null;
        AbrirCertificado.Visibility = equipo.RequiereCalibracion ? Visibility.Visible : Visibility.Collapsed;

        var historial = repo.Mantenimientos
            .Where(m => m.EquipoId == equipo.Id)
            .OrderByDescending(m => m.FechaProgramada)
            .Take(4)
            .Select(m => $"{m.Id} · {Formato.Tipo(m.Tipo)} · {Formato.FechaCorta(m.FechaProgramada)} · {Formato.Estado(repo.EstadoEfectivo(m))}")
            .ToList();
        if (historial.Count == 0)
            historial.Add("Sin órdenes registradas.");
        Historial.ItemsSource = historial;
    }

    private static string TextoCalibracion(Repositorio repo, Equipo equipo)
    {
        if (!equipo.RequiereCalibracion)
            return "Este equipo no entra en el plan de calibración.";

        var ultima = repo.UltimaCalibracion(equipo.Id);
        if (ultima is null)
            return "Sin certificado.";

        return $"{ultima.Id} · {Formato.Situacion(repo.Situacion(ultima))} · {Formato.Resultado(ultima.Resultado)} · próxima {Formato.FechaCorta(ultima.ProximaFecha)}";
    }

    private void AbrirCertificado_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null)
            return;
        var id = Repositorio.Instancia.UltimaCalibracion(_seleccionadoId)?.Id;
        if (id is not null)
            MainWindow.Instancia?.Navegar("calibracion", id);
    }

    private static FilaLista CrearFila(Repositorio repo, Equipo equipo)
    {
        var cal = equipo.RequiereCalibracion ? TextoCortoCalibracion(repo, equipo) : "Sin calibración";
        return new FilaLista
        {
            Id = equipo.Id,
            Titulo = equipo.Nombre,
            Detalle = $"{equipo.Id} · {equipo.Ubicacion} · {cal}",
            Estado = Formato.Estado(equipo.Estado),
            ColorEstado = Apariencia.De(equipo.Estado)
        };
    }

    private static string TextoCortoCalibracion(Repositorio repo, Equipo equipo)
    {
        var ultima = repo.UltimaCalibracion(equipo.Id);
        return ultima is null ? "Sin certificado" : Formato.Situacion(repo.Situacion(ultima));
    }

    private static bool Contiene(string texto, string busqueda) =>
        texto.Contains(busqueda, StringComparison.CurrentCultureIgnoreCase);

    private sealed class OpcionCategoria(string texto, CategoriaEquipo? categoria)
    {
        public CategoriaEquipo? Categoria { get; } = categoria;
        public override string ToString() => texto;
    }
}
