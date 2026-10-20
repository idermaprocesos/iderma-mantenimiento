using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Dialogos;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Pages;

public sealed partial class CalibracionesPage : Page
{
    public ObservableCollection<FilaLista> Filas { get; } = [];

    private string? _pendiente;
    private string? _seleccionadoId;

    public CalibracionesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.New && e.Parameter is string id && !string.IsNullOrEmpty(id))
            _pendiente = id;

        Repositorio.Instancia.Cambio += Cargar;
        Cargar();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Repositorio.Instancia.Cambio -= Cargar;
    }

    private void Cargar()
    {
        var repo = Repositorio.Instancia;
        var previo = _pendiente ?? _seleccionadoId ?? Repositorio.IdEjemploCalibracion;
        _pendiente = null;

        var calibraciones = repo.Calibraciones
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.Id);

        Filas.Clear();
        foreach (var calibracion in calibraciones)
            Filas.Add(CrearFila(repo, calibracion));

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
        var calibracion = Repositorio.Instancia.CalibracionPorId(fila.Id);
        if (calibracion is not null)
            Mostrar(calibracion);
    }

    private void Mostrar(Calibracion calibracion)
    {
        var repo = Repositorio.Instancia;
        var equipo = repo.EquipoPorId(calibracion.EquipoId);
        var situacion = repo.Situacion(calibracion);

        DetalleVacio.Visibility = Visibility.Collapsed;
        Detalle.Visibility = Visibility.Visible;
        Codigo.Text = calibracion.Id;
        Titulo.Text = equipo?.Nombre ?? calibracion.EquipoId;
        EquipoLinea.Text = equipo is null
            ? calibracion.EquipoId
            : $"{equipo.Id} · {equipo.Marca} {equipo.Modelo} · {equipo.Ubicacion}";
        FechaTexto.Text = Formato.Fecha(calibracion.Fecha);
        ProximaTexto.Text = $"{Formato.Fecha(calibracion.ProximaFecha)} · {Formato.Plazo(calibracion.ProximaFecha)}";
        Tecnico.Text = calibracion.Tecnico;
        Criterio.Text = calibracion.Criterio;
        Condiciones.Text = string.IsNullOrWhiteSpace(calibracion.CondicionesAmbientales)
            ? "Sin condiciones registradas."
            : calibracion.CondicionesAmbientales;
        TextoPatron.Text = calibracion.Patron;
        CertificadoPatron.Text = calibracion.CertificadoPatron;
        Procedimiento.Text = calibracion.Procedimiento;
        Observaciones.Text = string.IsNullOrWhiteSpace(calibracion.Observaciones)
            ? "Sin observaciones."
            : calibracion.Observaciones;
        VerEquipo.Content = equipo is null ? "Ver ficha del equipo" : $"Ver ficha de {equipo.Nombre}";

        TablaPuntos.ItemsSource = calibracion.Puntos.Select(p => new PuntoVista
        {
            Nombre = p.Nombre,
            Patron = Formato.Medida(p.ValorPatron, p.Unidad),
            Lectura = Formato.Medida(p.Lectura, p.Unidad),
            ErrorMedida = Formato.Error(p.Error, p.Unidad),
            Tolerancia = Formato.Tolerancia(p.Tolerancia, p.Unidad),
            Dictamen = p.Conforme ? "Conforme" : "No conforme",
            ColorDictamen = p.Conforme ? Apariencia.Exito() : Apariencia.Error(),
            Fondo = p.Conforme ? Apariencia.Transparente() : Apariencia.FondoError()
        }).ToList();

        AplicarAviso(calibracion, situacion);
    }

    private void AplicarAviso(Calibracion calibracion, SituacionCertificado situacion)
    {
        if (calibracion.Id == Repositorio.IdEjemploCalibracion && situacion == SituacionCertificado.Vigente)
        {
            ResultadoAviso.Severity = InfoBarSeverity.Success;
            ResultadoAviso.Title = "Ejemplo conforme y vigente";
            ResultadoAviso.Message = "Comparación en tres puntos del registrador de la nevera de folículos: 2 °C, 4 °C y 8 °C. El error mayor es +0,17 °C, dentro de ±0,3 °C.";
        }
        else if (situacion == SituacionCertificado.NoConforme)
        {
            ResultadoAviso.Severity = InfoBarSeverity.Error;
            ResultadoAviso.Title = "No conforme";
            ResultadoAviso.Message = "Hay puntos fuera de tolerancia. El equipo no debe usarse en clínica hasta el ajuste y una verificación nueva.";
        }
        else if (situacion == SituacionCertificado.Vencido)
        {
            ResultadoAviso.Severity = InfoBarSeverity.Warning;
            ResultadoAviso.Title = "Certificado vencido";
            ResultadoAviso.Message = "La comparación fue conforme, pero la fecha de la siguiente calibración ya pasó.";
        }
        else if (situacion == SituacionCertificado.PorVencer)
        {
            ResultadoAviso.Severity = InfoBarSeverity.Warning;
            ResultadoAviso.Title = "Por vencer";
            ResultadoAviso.Message = $"El certificado sigue vigente. Programa la siguiente comparación antes del {Formato.Fecha(calibracion.ProximaFecha)}.";
        }
        else if (situacion == SituacionCertificado.Historico)
        {
            ResultadoAviso.Severity = InfoBarSeverity.Informational;
            ResultadoAviso.Title = "Certificado histórico";
            ResultadoAviso.Message = $"Resultado de aquella comparación: {Formato.Resultado(calibracion.Resultado).ToLower(System.Globalization.CultureInfo.GetCultureInfo("es-ES"))}. Hay un certificado más reciente de este equipo.";
        }
        else
        {
            ResultadoAviso.Severity = InfoBarSeverity.Success;
            ResultadoAviso.Title = "Conforme y vigente";
            ResultadoAviso.Message = "Todos los puntos están dentro de tolerancia.";
        }

        ResultadoAviso.IsOpen = true;
    }

    private async void Nueva_Click(object sender, RoutedEventArgs e)
    {
        var dialogo = new DialogoCalibracion { XamlRoot = XamlRoot };
        if (await dialogo.ShowAsync() == ContentDialogResult.Primary && dialogo.IdCreado is string id)
        {
            _pendiente = id;
            Cargar();
        }
    }

    private void VerEquipo_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null)
            return;
        var equipoId = Repositorio.Instancia.CalibracionPorId(_seleccionadoId)?.EquipoId;
        if (equipoId is not null)
            MainWindow.Instancia?.Navegar("equipos", equipoId);
    }

    private async void ExportarCertificado_Click(object sender, RoutedEventArgs e)
    {
        if (_seleccionadoId is null || Repositorio.Instancia.CalibracionPorId(_seleccionadoId) is not Calibracion calibracion)
        {
            await AvisarAsync("Selecciona un certificado.");
            return;
        }

        await GuardarPdf.GuardarAsync(XamlRoot, calibracion.Id, ruta => ExportacionPdf.Certificado(calibracion, ruta));
    }

    private async void ExportarLista_Click(object sender, RoutedEventArgs e)
    {
        var calibraciones = Repositorio.Instancia.Calibraciones.ToList();
        if (calibraciones.Count == 0)
        {
            await AvisarAsync("No hay calibraciones para exportar.");
            return;
        }

        await GuardarPdf.GuardarAsync(XamlRoot, "Calibraciones", ruta => ExportacionPdf.Certificados(calibraciones, ruta));
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

    private static FilaLista CrearFila(Repositorio repo, Calibracion calibracion)
    {
        var situacion = repo.Situacion(calibracion);
        var equipo = repo.EquipoPorId(calibracion.EquipoId);
        return new FilaLista
        {
            Id = calibracion.Id,
            Titulo = equipo?.Nombre ?? calibracion.EquipoId,
            Detalle = $"{calibracion.Id} · {Formato.FechaCorta(calibracion.Fecha)}",
            Estado = Formato.Situacion(situacion),
            ColorEstado = Apariencia.De(situacion)
        };
    }
}
