using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace MantenimientoCapilar.Servicios;

public static class AperturaEquipo
{
    private static string? _equipoId;

    public static event Action<string>? Pedido;

    public static void Recibir(AppActivationArguments? args, bool usarLineaDeComandos)
    {
        var id = args is null ? null : DesdeActivacion(args);
        if (string.IsNullOrWhiteSpace(id) && usarLineaDeComandos)
            id = DesdeArgumentos(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
        if (string.IsNullOrWhiteSpace(id))
            return;

        _equipoId = id;
        Pedido?.Invoke(id);
    }

    public static string? Consumir()
    {
        var id = _equipoId;
        _equipoId = null;
        return id;
    }

    private static string? DesdeActivacion(AppActivationArguments args)
    {
        if (args.Kind == ExtendedActivationKind.Launch && args.Data is LaunchActivatedEventArgs datos)
            return DesdeArgumentos(datos.Arguments);

        return null;
    }

    public static string? DesdeArgumentos(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        var partes = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < partes.Length; i++)
        {
            var parte = partes[i].TrimStart('-');
            if (parte.StartsWith("equipo=", StringComparison.OrdinalIgnoreCase))
                return parte["equipo=".Length..];

            if (parte.Equals("equipo", StringComparison.OrdinalIgnoreCase) && i + 1 < partes.Length)
                return partes[i + 1];
        }

        return null;
    }
}
