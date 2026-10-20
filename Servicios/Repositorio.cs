using System.Text.Json;
using System.Text.Json.Serialization;
using MantenimientoCapilar.Modelos;

namespace MantenimientoCapilar.Servicios;

public sealed class Repositorio
{
    public const string IdEjemploCalibracion = "CAL-2026-018";

    public static Repositorio Instancia { get; } = new();

    private static readonly JsonSerializerOptions Opciones = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private ArchivoDatos _datos = new();

    public string Ruta { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Iderma",
        "MantenimientoCapilar",
        "datos.json");

    public event Action? Cambio;

    private Repositorio()
    {
    }

    public IReadOnlyList<Equipo> Equipos => _datos.Equipos;
    public IReadOnlyList<OrdenMantenimiento> Mantenimientos => _datos.Mantenimientos;
    public IReadOnlyList<Calibracion> Calibraciones => _datos.Calibraciones;

    public void Cargar()
    {
        try
        {
            if (File.Exists(Ruta))
            {
                var datos = JsonSerializer.Deserialize<ArchivoDatos>(File.ReadAllText(Ruta), Opciones);
                if (datos is { Equipos.Count: > 0 })
                {
                    _datos = datos;
                    return;
                }
            }
        }
        catch (JsonException)
        {
        }
        catch (IOException)
        {
        }

        _datos = DatosEjemplo.Crear();
        Guardar();
    }

    public void RestaurarEjemplo()
    {
        _datos = DatosEjemplo.Crear();
        Guardar();
        Cambio?.Invoke();
    }

    public Equipo? EquipoPorId(string id) => _datos.Equipos.FirstOrDefault(e => e.Id == id);

    public OrdenMantenimiento? OrdenPorId(string id) => _datos.Mantenimientos.FirstOrDefault(o => o.Id == id);

    public Calibracion? CalibracionPorId(string id) => _datos.Calibraciones.FirstOrDefault(c => c.Id == id);

    public Calibracion? UltimaCalibracion(string equipoId) =>
        _datos.Calibraciones
            .Where(c => c.EquipoId == equipoId)
            .OrderByDescending(c => c.Fecha)
            .ThenByDescending(c => c.Id)
            .FirstOrDefault();

    public EstadoOrden EstadoEfectivo(OrdenMantenimiento orden)
    {
        if (orden.Estado is EstadoOrden.Completado or EstadoOrden.EnCurso)
            return orden.Estado;

        return orden.FechaProgramada.Date < DateTime.Today
            ? EstadoOrden.Vencido
            : EstadoOrden.Programado;
    }

    public SituacionCertificado Situacion(Calibracion calibracion)
    {
        var ultima = UltimaCalibracion(calibracion.EquipoId);
        if (ultima is null || ultima.Id != calibracion.Id)
            return SituacionCertificado.Historico;

        if (calibracion.Resultado == ResultadoCalibracion.NoConforme)
            return SituacionCertificado.NoConforme;

        var dias = (calibracion.ProximaFecha.Date - DateTime.Today).Days;
        if (dias < 0)
            return SituacionCertificado.Vencido;
        if (dias <= 30)
            return SituacionCertificado.PorVencer;
        return SituacionCertificado.Vigente;
    }

    public int ContarMantenimientosVencidos() =>
        _datos.Mantenimientos.Count(m => EstadoEfectivo(m) == EstadoOrden.Vencido);

    public int ContarCalibracionesEnAlerta() =>
        _datos.Equipos.Count(equipo =>
        {
            if (!equipo.RequiereCalibracion)
                return false;

            var ultima = UltimaCalibracion(equipo.Id);
            if (ultima is null)
                return true;

            return Situacion(ultima) is SituacionCertificado.NoConforme
                or SituacionCertificado.Vencido
                or SituacionCertificado.PorVencer;
        });

    public string AgregarMantenimiento(
        string equipoId,
        TipoMantenimiento tipo,
        DateTime fecha,
        string tecnico,
        string descripcion)
    {
        var orden = new OrdenMantenimiento
        {
            Id = NuevoId("MT-", _datos.Mantenimientos.Select(m => m.Id), 4),
            EquipoId = equipoId,
            Tipo = tipo,
            FechaProgramada = fecha.Date,
            Tecnico = tecnico.Trim(),
            Descripcion = descripcion.Trim(),
            Hallazgos = "",
            Estado = EstadoOrden.Programado
        };
        _datos.Mantenimientos.Add(orden);
        Guardar();
        Cambio?.Invoke();
        return orden.Id;
    }

    public void CompletarMantenimiento(string id, string hallazgos)
    {
        var orden = OrdenPorId(id);
        if (orden is null || orden.Estado == EstadoOrden.Completado)
            return;

        orden.Estado = EstadoOrden.Completado;
        orden.FechaEjecucion = DateTime.Today;
        orden.Hallazgos = hallazgos.Trim();

        var equipo = EquipoPorId(orden.EquipoId);
        if (equipo is { Estado: EstadoEquipo.EnMantenimiento } && !SigueFueraDeUso(equipo))
            equipo.Estado = EstadoEquipo.Operativo;

        Guardar();
        Cambio?.Invoke();
    }

    public string AgregarCalibracion(Calibracion calibracion)
    {
        if (string.IsNullOrWhiteSpace(calibracion.Id))
        {
            calibracion.Id = NuevoId(
                $"CAL-{calibracion.Fecha.Year}-",
                _datos.Calibraciones.Select(c => c.Id),
                3);
        }

        _datos.Calibraciones.Add(calibracion);

        var equipo = EquipoPorId(calibracion.EquipoId);
        if (equipo is not null)
        {
            if (calibracion.Resultado == ResultadoCalibracion.NoConforme)
            {
                equipo.Estado = EstadoEquipo.EnMantenimiento;
            }
            else if (equipo.Estado == EstadoEquipo.EnMantenimiento && !SigueFueraDeUso(equipo))
            {
                equipo.Estado = EstadoEquipo.Operativo;
            }
        }

        Guardar();
        Cambio?.Invoke();
        return calibracion.Id;
    }

    private bool SigueFueraDeUso(Equipo equipo)
    {
        var enCurso = _datos.Mantenimientos.Any(m =>
            m.EquipoId == equipo.Id && m.Estado == EstadoOrden.EnCurso);
        if (enCurso)
            return true;

        if (!equipo.RequiereCalibracion)
            return false;

        return UltimaCalibracion(equipo.Id) is { Resultado: ResultadoCalibracion.NoConforme };
    }

    private static string NuevoId(string prefijo, IEnumerable<string> ids, int cifras)
    {
        var maximo = 0;
        foreach (var id in ids)
        {
            if (!id.StartsWith(prefijo, StringComparison.Ordinal))
                continue;
            if (int.TryParse(id[prefijo.Length..], out var numero))
                maximo = Math.Max(maximo, numero);
        }

        return prefijo + (maximo + 1).ToString("D" + cifras);
    }

    private void Guardar()
    {
        var carpeta = Path.GetDirectoryName(Ruta);
        if (!string.IsNullOrEmpty(carpeta))
            Directory.CreateDirectory(carpeta);

        File.WriteAllText(Ruta, JsonSerializer.Serialize(_datos, Opciones));
    }
}
