using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MantenimientoCapilar.Servicios;

public static class GuardarPdf
{
    public static async Task GuardarAsync(XamlRoot raiz, string nombre, Action<string> generar)
    {
        var ventana = MainWindow.Instancia;
        if (ventana is null)
            return;

        var selector = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = Path.GetFileNameWithoutExtension(nombre)
        };
        selector.FileTypeChoices.Add("Documento PDF", new List<string> { ".pdf" });

        InitializeWithWindow.Initialize(selector, WindowNative.GetWindowHandle(ventana));
        var archivo = await selector.PickSaveFileAsync();
        if (archivo is null)
            return;

        try
        {
            generar(archivo.Path);
            Process.Start(new ProcessStartInfo(archivo.Path) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            var dialogo = new ContentDialog
            {
                Title = "No se pudo crear el PDF",
                Content = ex.Message,
                CloseButtonText = "Cerrar",
                XamlRoot = raiz
            };
            await dialogo.ShowAsync();
        }
    }
}
