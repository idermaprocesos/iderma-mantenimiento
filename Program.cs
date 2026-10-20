using System.Runtime.InteropServices;
using MantenimientoCapilar.Servicios;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace MantenimientoCapilar;

public static class Program
{
    private const string ClaveInstancia = "MantenimientoCapilar";

    [DllImport("Microsoft.WindowsAppRuntime.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int WindowsAppRuntime_EnsureIsLoaded();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetEnvironmentVariable(string lpName, string lpValue);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();

    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(uint dwProcessId);

    [STAThread]
    private static void Main(string[] args)
    {
        SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
        SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY_PID", GetCurrentProcessId().ToString());
        _ = WindowsAppRuntime_EnsureIsLoaded();

        if (RedirigirSiYaEstaAbierta())
            return;

        Application.Start(p =>
        {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }

    private static bool RedirigirSiYaEstaAbierta()
    {
        try
        {
            var actual = AppInstance.GetCurrent();
            var argumentos = actual.GetActivatedEventArgs();
            var principal = AppInstance.FindOrRegisterForKey(ClaveInstancia);
            if (principal.IsCurrent)
            {
                principal.Activated += (_, e) => AperturaEquipo.Recibir(e, false);
                AperturaEquipo.Recibir(argumentos, true);
                return false;
            }

            AllowSetForegroundWindow(principal.ProcessId);
            using var listo = new Semaphore(0, 1);
            Task.Run(() =>
            {
                try
                {
                    principal.RedirectActivationToAsync(argumentos).AsTask().Wait();
                }
                finally
                {
                    listo.Release();
                }
            });
            listo.WaitOne();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
