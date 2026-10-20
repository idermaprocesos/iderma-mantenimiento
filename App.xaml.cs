using Microsoft.UI.Xaml;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Repositorio.Instancia.Cargar();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var ventana = new MainWindow();
        ventana.Activate();
    }
}
