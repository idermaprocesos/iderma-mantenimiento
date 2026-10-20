using System.Globalization;
using MantenimientoCapilar.Modelos;

namespace MantenimientoCapilar.Presentacion;

public static class Formato
{
    private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

    public static string Fecha(DateTime fecha) => fecha.ToString("d 'de' MMMM 'de' yyyy", Es);

    public static string FechaCorta(DateTime fecha) => fecha.ToString("dd MMM yyyy", Es);

    public static string Plazo(DateTime fecha)
    {
        var dias = (fecha.Date - DateTime.Today).Days;
        return dias switch
        {
            0 => "hoy",
            1 => "mañana",
            -1 => "ayer",
            > 1 => $"en {dias} días",
            _ => $"hace {-dias} días"
        };
    }

    public static string Medida(double valor, string unidad)
    {
        var texto = unidad == "rpm"
            ? valor.ToString("N0", Es)
            : valor.ToString("0.###", Es);
        return string.IsNullOrWhiteSpace(unidad) ? texto : $"{texto} {unidad}";
    }

    public static string Error(double valor, string unidad)
    {
        var texto = unidad == "rpm"
            ? valor.ToString("+0;-0;0", Es)
            : valor.ToString("+0.###;-0.###;0", Es);
        return string.IsNullOrWhiteSpace(unidad) ? texto : $"{texto} {unidad}";
    }

    public static string Tolerancia(double valor, string unidad) => $"±{Medida(valor, unidad)}";

    public static string Categoria(CategoriaEquipo categoria) => categoria switch
    {
        CategoriaEquipo.Extraccion => "Extracción FUE",
        CategoriaEquipo.Implantacion => "Implantación",
        CategoriaEquipo.Diagnostico => "Diagnóstico",
        CategoriaEquipo.Esterilizacion => "Esterilización",
        CategoriaEquipo.Laboratorio => "Laboratorio",
        CategoriaEquipo.CadenaFrio => "Cadena de frío",
        CategoriaEquipo.Fototerapia => "Fototerapia",
        CategoriaEquipo.Quirofano => "Quirófano",
        _ => categoria.ToString()
    };

    public static string Estado(EstadoEquipo estado) => estado switch
    {
        EstadoEquipo.Operativo => "Operativo",
        EstadoEquipo.EnMantenimiento => "En mantenimiento",
        EstadoEquipo.FueraDeServicio => "Fuera de servicio",
        _ => estado.ToString()
    };

    public static string Tipo(TipoMantenimiento tipo) => tipo switch
    {
        TipoMantenimiento.Preventivo => "Preventivo",
        TipoMantenimiento.Correctivo => "Correctivo",
        TipoMantenimiento.Limpieza => "Limpieza",
        TipoMantenimiento.Verificacion => "Verificación",
        _ => tipo.ToString()
    };

    public static string Estado(EstadoOrden estado) => estado switch
    {
        EstadoOrden.Programado => "Programado",
        EstadoOrden.EnCurso => "En curso",
        EstadoOrden.Completado => "Completado",
        EstadoOrden.Vencido => "Vencido",
        _ => estado.ToString()
    };

    public static string Situacion(SituacionCertificado situacion) => situacion switch
    {
        SituacionCertificado.Historico => "Histórico",
        SituacionCertificado.Vigente => "Vigente",
        SituacionCertificado.PorVencer => "Por vencer",
        SituacionCertificado.Vencido => "Vencida",
        SituacionCertificado.NoConforme => "No conforme",
        _ => situacion.ToString()
    };

    public static string Resultado(ResultadoCalibracion resultado) => resultado switch
    {
        ResultadoCalibracion.Conforme => "Conforme",
        ResultadoCalibracion.NoConforme => "No conforme",
        _ => resultado.ToString()
    };
}

public sealed class FilaLista
{
    public string Id { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string Detalle { get; set; } = "";
    public string Estado { get; set; } = "";
    public Microsoft.UI.Xaml.Media.Brush ColorEstado { get; set; } = null!;
}

public sealed class PuntoVista
{
    public string Nombre { get; set; } = "";
    public string Patron { get; set; } = "";
    public string Lectura { get; set; } = "";
    public string ErrorMedida { get; set; } = "";
    public string Tolerancia { get; set; } = "";
    public string Dictamen { get; set; } = "";
    public Microsoft.UI.Xaml.Media.Brush ColorDictamen { get; set; } = null!;
    public Microsoft.UI.Xaml.Media.Brush Fondo { get; set; } = null!;
}
