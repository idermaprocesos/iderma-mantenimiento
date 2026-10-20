using Microsoft.UI.Xaml.Controls;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Dialogos;

public sealed partial class DialogoCalibracion : ContentDialog
{
    public string? IdCreado { get; private set; }

    public DialogoCalibracion()
    {
        InitializeComponent();
        var equipos = Repositorio.Instancia.Equipos.Where(e => e.RequiereCalibracion).OrderBy(e => e.Id).ToList();
        Equipos.ItemsSource = equipos;
        Equipos.SelectedItem = equipos.FirstOrDefault(e => e.Id == "EQ-011") ?? equipos.FirstOrDefault();
        Fecha.Date = DateTimeOffset.Now;
        Tecnico.Text = "Metrología interna";
        ActualizarVistaPrevia();
    }

    private void Equipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Equipos.SelectedItem is not Equipo equipo)
            return;

        if (equipo.Id == "EQ-009")
        {
            Unidad.Text = "rpm";
            Tolerancia.Value = 70;
            Criterio.Text = "±2 % de la consigna de velocidad.";
        }
        else if (equipo.Id == "EQ-008")
        {
            Unidad.Text = "g";
            Tolerancia.Value = 0.02;
            Criterio.Text = "±0,02 g en el rango de uso.";
        }
        else
        {
            Unidad.Text = "°C";
            Tolerancia.Value = 0.3;
            Criterio.Text = "±0,3 °C en la banda de conservación de folículos.";
        }

        ActualizarVistaPrevia();
    }

    private void Dato_Changed(NumberBox sender, NumberBoxValueChangedEventArgs args) => ActualizarVistaPrevia();

    private void DatoTexto_Changed(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e) => ActualizarVistaPrevia();

    private void Guardar_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (Equipos.SelectedItem is not Equipo equipo || Fecha.Date is not DateTimeOffset fecha)
        {
            args.Cancel = true;
            MostrarError("Elige el equipo y la fecha.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Patron.Text))
        {
            args.Cancel = true;
            MostrarError("Indica el patrón usado en la comparación.");
            return;
        }

        var tolerancia = Leer(Tolerancia);
        if (tolerancia <= 0)
        {
            args.Cancel = true;
            MostrarError("La tolerancia tiene que ser mayor que cero.");
            return;
        }

        var puntos = LeerPuntos(tolerancia, Unidad.Text.Trim());
        if (puntos.Count == 0)
        {
            args.Cancel = true;
            MostrarError("Añade al menos un punto con nombre, valor patrón y lectura.");
            return;
        }

        var borrador = new Calibracion { Puntos = puntos };
        var intervalo = Math.Max(equipo.IntervaloCalibracionDias, 30);
        var proxima = borrador.Resultado == ResultadoCalibracion.NoConforme
            ? fecha.Date
            : fecha.Date.AddDays(intervalo);

        var calibracion = new Calibracion
        {
            EquipoId = equipo.Id,
            Fecha = fecha.Date,
            ProximaFecha = proxima,
            Tecnico = string.IsNullOrWhiteSpace(Tecnico.Text) ? "Metrología interna" : Tecnico.Text.Trim(),
            Patron = Patron.Text.Trim(),
            CertificadoPatron = CertificadoPatron.Text.Trim(),
            Procedimiento = string.IsNullOrWhiteSpace(Procedimiento.Text)
                ? "Comparación contra patrón en los puntos indicados."
                : Procedimiento.Text.Trim(),
            CondicionesAmbientales = Condiciones.Text.Trim(),
            Unidad = Unidad.Text.Trim(),
            Criterio = string.IsNullOrWhiteSpace(Criterio.Text)
                ? FormatoTolerancia(tolerancia, Unidad.Text.Trim())
                : Criterio.Text.Trim(),
            Observaciones = Observaciones.Text.Trim(),
            Puntos = puntos
        };

        IdCreado = Repositorio.Instancia.AgregarCalibracion(calibracion);
    }

    private void MostrarError(string mensaje)
    {
        Aviso.Severity = InfoBarSeverity.Error;
        Aviso.Title = "No se puede guardar";
        Aviso.Message = mensaje;
        Aviso.IsOpen = true;
    }

    private void ActualizarVistaPrevia()
    {
        if (Aviso is null || Tolerancia is null)
            return;

        var puntos = LeerPuntos(Leer(Tolerancia), Unidad.Text.Trim());
        if (puntos.Count == 0)
        {
            Aviso.Severity = InfoBarSeverity.Informational;
            Aviso.Title = "Dictamen preliminar";
            Aviso.Message = "Cuando completes un punto verás si queda dentro de tolerancia. Error = lectura − patrón.";
            Aviso.IsOpen = true;
            return;
        }

        var fuera = puntos.Where(p => !p.Conforme).Select(p => p.Nombre).ToList();
        if (fuera.Count == 0)
        {
            Aviso.Severity = InfoBarSeverity.Success;
            Aviso.Title = "Conforme";
            Aviso.Message = "Todos los puntos indicados están dentro de tolerancia.";
        }
        else
        {
            Aviso.Severity = InfoBarSeverity.Error;
            Aviso.Title = "No conforme";
            Aviso.Message = "Fuera de tolerancia: " + string.Join(", ", fuera) + ".";
        }

        Aviso.IsOpen = true;
    }

    private List<PuntoCalibracion> LeerPuntos(double tolerancia, string unidad)
    {
        var puntos = new List<PuntoCalibracion>();
        Agregar(puntos, Nombre1.Text, Patron1, Lectura1, tolerancia, unidad);
        Agregar(puntos, Nombre2.Text, Patron2, Lectura2, tolerancia, unidad);
        Agregar(puntos, Nombre3.Text, Patron3, Lectura3, tolerancia, unidad);
        return puntos;
    }

    private static void Agregar(
        List<PuntoCalibracion> puntos,
        string nombre,
        NumberBox patron,
        NumberBox lectura,
        double tolerancia,
        string unidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return;

        puntos.Add(new PuntoCalibracion
        {
            Nombre = nombre.Trim(),
            ValorPatron = Leer(patron),
            Lectura = Leer(lectura),
            Tolerancia = tolerancia,
            Unidad = unidad
        });
    }

    private static double Leer(NumberBox caja) => double.IsNaN(caja.Value) ? 0 : caja.Value;

    private static string FormatoTolerancia(double tolerancia, string unidad) =>
        string.IsNullOrWhiteSpace(unidad) ? $"±{tolerancia}" : $"±{tolerancia} {unidad}";
}
