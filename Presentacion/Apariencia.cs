using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using MantenimientoCapilar.Modelos;

namespace MantenimientoCapilar.Presentacion;

public static class Apariencia
{
    public static Brush Exito() => Buscar("SystemFillColorSuccessBrush", Colors.ForestGreen);
    public static Brush Aviso() => Buscar("SystemFillColorCautionBrush", Colors.DarkOrange);
    public static Brush Error() => Buscar("SystemFillColorCriticalBrush", Colors.Firebrick);
    public static Brush Neutro() => Buscar("SystemFillColorNeutralBrush", Colors.Gray);
    public static Brush Atencion() => Buscar("SystemFillColorAttentionBrush", Colors.DodgerBlue);
    public static Brush FondoError() => Buscar("SystemFillColorCriticalBackgroundBrush", global::Windows.UI.Color.FromArgb(36, 196, 43, 28));
    public static Brush Transparente() => new SolidColorBrush(Colors.Transparent);

    public static Brush De(EstadoEquipo estado) => estado switch
    {
        EstadoEquipo.Operativo => Exito(),
        EstadoEquipo.EnMantenimiento => Aviso(),
        EstadoEquipo.FueraDeServicio => Error(),
        _ => Neutro()
    };

    public static Brush De(EstadoOrden estado) => estado switch
    {
        EstadoOrden.Completado => Exito(),
        EstadoOrden.EnCurso => Aviso(),
        EstadoOrden.Vencido => Error(),
        EstadoOrden.Programado => Atencion(),
        _ => Neutro()
    };

    public static Brush De(SituacionCertificado situacion) => situacion switch
    {
        SituacionCertificado.Vigente => Exito(),
        SituacionCertificado.PorVencer => Aviso(),
        SituacionCertificado.Vencido => Error(),
        SituacionCertificado.NoConforme => Error(),
        _ => Neutro()
    };

    private static Brush Buscar(string clave, global::Windows.UI.Color respaldo)
    {
        if (Application.Current.Resources.TryGetValue(clave, out var valor) && valor is Brush brocha)
            return brocha;

        return new SolidColorBrush(respaldo);
    }
}
