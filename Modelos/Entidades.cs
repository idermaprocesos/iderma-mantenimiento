namespace MantenimientoCapilar.Modelos;

public enum CategoriaEquipo
{
    Extraccion,
    Implantacion,
    Diagnostico,
    Esterilizacion,
    Laboratorio,
    CadenaFrio,
    Fototerapia,
    Quirofano
}

public enum EstadoEquipo
{
    Operativo,
    EnMantenimiento,
    FueraDeServicio
}

public enum TipoMantenimiento
{
    Preventivo,
    Correctivo,
    Limpieza,
    Verificacion
}

public enum EstadoOrden
{
    Programado,
    EnCurso,
    Completado,
    Vencido
}

public enum ResultadoCalibracion
{
    Conforme,
    NoConforme
}

public enum SituacionCertificado
{
    Historico,
    Vigente,
    PorVencer,
    Vencido,
    NoConforme
}

public sealed class Equipo
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string NumeroSerie { get; set; } = "";
    public CategoriaEquipo Categoria { get; set; }
    public string Ubicacion { get; set; } = "";
    public EstadoEquipo Estado { get; set; }
    public string Responsable { get; set; } = "";
    public bool RequiereCalibracion { get; set; }
    public int IntervaloMantenimientoDias { get; set; }
    public int IntervaloCalibracionDias { get; set; }
    public string Notas { get; set; } = "";

    public override string ToString() => $"{Id} · {Nombre}";
}

public sealed class OrdenMantenimiento
{
    public string Id { get; set; } = "";
    public string EquipoId { get; set; } = "";
    public TipoMantenimiento Tipo { get; set; }
    public DateTime FechaProgramada { get; set; }
    public DateTime? FechaEjecucion { get; set; }
    public string Tecnico { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public string Hallazgos { get; set; } = "";
    public EstadoOrden Estado { get; set; }
}

public sealed class PuntoCalibracion
{
    public string Nombre { get; set; } = "";
    public double ValorPatron { get; set; }
    public double Lectura { get; set; }
    public double Tolerancia { get; set; }
    public string Unidad { get; set; } = "";

    public double Error => Lectura - ValorPatron;

    public bool Conforme => Math.Abs(Error) <= Tolerancia + 0.0000001;
}

public sealed class Calibracion
{
    public string Id { get; set; } = "";
    public string EquipoId { get; set; } = "";
    public DateTime Fecha { get; set; }
    public DateTime ProximaFecha { get; set; }
    public string Tecnico { get; set; } = "";
    public string Patron { get; set; } = "";
    public string CertificadoPatron { get; set; } = "";
    public string Procedimiento { get; set; } = "";
    public string CondicionesAmbientales { get; set; } = "";
    public string Unidad { get; set; } = "";
    public string Criterio { get; set; } = "";
    public string Observaciones { get; set; } = "";
    public List<PuntoCalibracion> Puntos { get; set; } = [];

    public ResultadoCalibracion Resultado =>
        Puntos.Count > 0 && Puntos.All(p => p.Conforme)
            ? ResultadoCalibracion.Conforme
            : ResultadoCalibracion.NoConforme;
}

public sealed class ArchivoDatos
{
    public int Version { get; set; } = 1;
    public List<Equipo> Equipos { get; set; } = [];
    public List<OrdenMantenimiento> Mantenimientos { get; set; } = [];
    public List<Calibracion> Calibraciones { get; set; } = [];
}
