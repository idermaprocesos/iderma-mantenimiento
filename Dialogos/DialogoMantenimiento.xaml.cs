using Microsoft.UI.Xaml.Controls;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Dialogos;

public sealed partial class DialogoMantenimiento : ContentDialog
{
    public string? IdCreado { get; private set; }

    private readonly List<TipoMantenimiento> _tipos =
    [
        TipoMantenimiento.Preventivo,
        TipoMantenimiento.Correctivo,
        TipoMantenimiento.Limpieza,
        TipoMantenimiento.Verificacion
    ];

    public DialogoMantenimiento()
    {
        InitializeComponent();
        Equipos.ItemsSource = Repositorio.Instancia.Equipos.OrderBy(e => e.Id).ToList();
        Tipos.ItemsSource = _tipos.Select(Formato.Tipo).ToList();
        Tipos.SelectedIndex = 0;
        Fecha.Date = DateTimeOffset.Now;
        Tecnico.Text = "Técnico biomédico";
    }

    private void Guardar_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (Equipos.SelectedItem is not Equipo equipo || string.IsNullOrWhiteSpace(Descripcion.Text))
        {
            args.Cancel = true;
            Aviso.Message = "Elige un equipo y describe el trabajo.";
            Aviso.IsOpen = true;
            return;
        }

        if (Fecha.Date is not DateTimeOffset fecha)
        {
            args.Cancel = true;
            Aviso.Message = "Indica la fecha programada.";
            Aviso.IsOpen = true;
            return;
        }

        var tipo = _tipos[Tipos.SelectedIndex < 0 ? 0 : Tipos.SelectedIndex];
        IdCreado = Repositorio.Instancia.AgregarMantenimiento(
            equipo.Id,
            tipo,
            fecha.Date,
            Tecnico.Text,
            Descripcion.Text);
    }
}
