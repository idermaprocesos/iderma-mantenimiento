using MantenimientoCapilar.Modelos;

namespace MantenimientoCapilar.Servicios;

public static class DatosEjemplo
{
    public static ArchivoDatos Crear()
    {
        return new ArchivoDatos
        {
            Version = 1,
            Equipos = Equipos(),
            Mantenimientos = Mantenimientos(),
            Calibraciones = Calibraciones()
        };
    }

    private static List<Equipo> Equipos() =>
    [
        Equipo("EQ-001", "Unidad de extracción FUE", "Devroye Instruments", "WAW", "WAW-1842",
            CategoriaEquipo.Extraccion, "Sala FUE 1", EstadoEquipo.Operativo, "Enfermería quirúrgica",
            false, 180, 0, "Motor de extracción y pedal. Los punches se controlan como instrumental aparte."),
        Equipo("EQ-002", "Set de punches híbridos 0,8–1,0 mm", "Devroye Instruments", "Hybrid Punch", "HP-26-014",
            CategoriaEquipo.Extraccion, "Sala FUE 1", EstadoEquipo.Operativo, "Enfermería quirúrgica",
            false, 90, 0, "Sustituir el filo cuando el corte folicular deje de ser limpio."),
        Equipo("EQ-003", "Implanters Choi 0,8 / 0,9 / 1,0 mm", "LeadM", "Choi", "CHOI-3391",
            CategoriaEquipo.Implantacion, "Sala DHI", EstadoEquipo.Operativo, "Enfermería quirúrgica",
            false, 30, 0, "Limpieza y revisión del muelle después de cada jornada de implantación."),
        Equipo("EQ-004", "Microscopio estereoscópico de corte", "Olympus", "SZ61", "SZ61-22091",
            CategoriaEquipo.Diagnostico, "Laboratorio de injertos", EstadoEquipo.Operativo, "Laboratorio de injertos",
            false, 180, 0, "Corte y conteo folicular. Mantener la óptica libre de solución de conservación."),
        Equipo("EQ-005", "Dermatoscopio digital", "FotoFinder", "Vexia", "FF-55210",
            CategoriaEquipo.Diagnostico, "Consultorio 2", EstadoEquipo.Operativo, "Consulta",
            false, 365, 0, "Valoración de densidad y zona donante."),
        Equipo("EQ-006", "Autoclave clase B 18 L", "Tuttnauer", "Elara 11", "EL-77821",
            CategoriaEquipo.Esterilizacion, "Central de esterilización", EstadoEquipo.Operativo, "Esterilización",
            false, 90, 0, "Ciclo de instrumental hueco. Indicador biológico en el preventivo trimestral."),
        Equipo("EQ-007", "Selladora de pouches", "Euronda", "Euroseal", "ES-1902",
            CategoriaEquipo.Esterilizacion, "Central de esterilización", EstadoEquipo.Operativo, "Esterilización",
            false, 180, 0, "Comprobar temperatura de mordazas y continuidad del sello."),
        Equipo("EQ-008", "Balanza de precisión", "Ohaus", "Pioneer PX2202", "PX-883410",
            CategoriaEquipo.Laboratorio, "Preparación de soluciones", EstadoEquipo.Operativo, "Laboratorio",
            true, 180, 365, "Pesada de soluciones y comprobación con masas de referencia."),
        Equipo("EQ-009", "Centrífuga de PRP", "Hettich", "EBA 200", "EB-44190",
            CategoriaEquipo.Laboratorio, "Laboratorio PRP", EstadoEquipo.EnMantenimiento, "Laboratorio",
            true, 180, 180, "Fuera de uso clínico hasta corregir la desviación de velocidad y repetir la verificación."),
        Equipo("EQ-010", "Nevera de conservación de folículos", "Liebherr", "MediLine", "LB-22018",
            CategoriaEquipo.CadenaFrio, "Sala de injertos", EstadoEquipo.Operativo, "Laboratorio de injertos",
            false, 90, 0, "Consigna 4 °C. Alarma si sale del intervalo 2–8 °C."),
        Equipo("EQ-011", "Termómetro registrador de cadena de frío", "Testo", "175 T1", "TE-175-6621",
            CategoriaEquipo.CadenaFrio, "Nevera de folículos, estante medio", EstadoEquipo.Operativo, "Laboratorio de injertos",
            true, 365, 365, "Canal de registro continuo de la nevera donde se conservan los folículos."),
        Equipo("EQ-012", "Lámpara de fotobiomodulación", "Capillus", "Pro", "CP-11820",
            CategoriaEquipo.Fototerapia, "Cabina de recuperación", EstadoEquipo.Operativo, "Recuperación",
            false, 180, 0, "Revisar diodos y tiempo de sesión antes de cada bloque de tratamientos."),
        Equipo("EQ-013", "Monitor multiparamétrico", "Mindray", "ePM 12", "MR-90211",
            CategoriaEquipo.Quirofano, "Quirófano", EstadoEquipo.Operativo, "Quirófano",
            false, 365, 0, "Constantes durante extracción e implantación."),
        Equipo("EQ-014", "Aspirador quirúrgico", "New Askir", "30", "AS-774",
            CategoriaEquipo.Quirofano, "Quirófano", EstadoEquipo.Operativo, "Quirófano",
            false, 180, 0, "Revisión de vacío, frasco y filtro."),
        Equipo("EQ-015", "Equipo de mesoterapia", "MesoJet", "U225", "MJ-225-19",
            CategoriaEquipo.Quirofano, "Consultorio 1", EstadoEquipo.EnMantenimiento, "Consulta",
            false, 120, 0, "Fuga en la pieza de mano. Consulta de mesoterapia suspendida.")
    ];

    private static List<OrdenMantenimiento> Mantenimientos() =>
    [
        Orden("MT-1048", "EQ-010", TipoMantenimiento.Preventivo, new DateTime(2026, 9, 29), null,
            "Técnico biomédico", EstadoOrden.Programado,
            "Comprobar alarma de puerta, junta y que el registro de temperatura sigue alineado con el termómetro calibrado.",
            ""),
        Orden("MT-1041", "EQ-006", TipoMantenimiento.Preventivo, new DateTime(2026, 9, 24), null,
            "Esterilización", EstadoOrden.Programado,
            "Preventivo trimestral: prueba Bowie-Dick, indicador biológico y revisión de juntas y depósito.",
            ""),
        Orden("MT-1044", "EQ-009", TipoMantenimiento.Correctivo, new DateTime(2026, 9, 8), null,
            "Técnico biomédico", EstadoOrden.EnCurso,
            "La verificación de velocidad salió no conforme en la consigna de PRP estándar. Equipo retirado de uso clínico.",
            "Pendiente de ajuste y de una calibración nueva antes de volver a centrifugar PRP."),
        Orden("MT-1038", "EQ-001", TipoMantenimiento.Preventivo, new DateTime(2026, 9, 15), null,
            "Enfermería quirúrgica", EstadoOrden.Programado,
            "Revisión de pedal, cable de motor y velocidad en vacío. La fecha de servicio ya pasó.",
            ""),
        Orden("MT-1036", "EQ-015", TipoMantenimiento.Correctivo, new DateTime(2026, 9, 18), null,
            "Técnico biomédico", EstadoOrden.EnCurso,
            "Fuga en el circuito de la pieza de mano. No usar en consulta hasta el cierre de la orden.",
            "Pieza de mano desmontada. A la espera de junta de recambio."),
        Orden("MT-1033", "EQ-004", TipoMantenimiento.Limpieza, new DateTime(2026, 9, 2), new DateTime(2026, 9, 2),
            "Laboratorio de injertos", EstadoOrden.Completado,
            "Limpieza de óptica y revisión del enfoque antes de la jornada de corte.",
            "Óptica limpia, sin holgura en el enfoque. Apto para corte folicular."),
        Orden("MT-1028", "EQ-007", TipoMantenimiento.Preventivo, new DateTime(2026, 8, 12), new DateTime(2026, 8, 12),
            "Esterilización", EstadoOrden.Completado,
            "Prueba de sello y temperatura de mordazas.",
            "Sellos íntegros. Temperatura de mordazas dentro del rango de uso."),
        Orden("MT-1021", "EQ-005", TipoMantenimiento.Verificacion, new DateTime(2026, 8, 4), new DateTime(2026, 8, 4),
            "Consulta", EstadoOrden.Completado,
            "Limpieza de lente y comprobación de la luz polarizada.",
            "Iluminación uniforme. Lente sin rayas.")
    ];

    private static List<Calibracion> Calibraciones() =>
    [
        new Calibracion
        {
            Id = Repositorio.IdEjemploCalibracion,
            EquipoId = "EQ-011",
            Fecha = new DateTime(2026, 3, 18),
            ProximaFecha = new DateTime(2027, 3, 18),
            Tecnico = "Metrología interna",
            Patron = "Termómetro de resistencia Fluke 1523 con sonda 5610-9.",
            CertificadoPatron = "LAC-PT-2026-044 · trazable a CENAM · patrón vigente hasta el 12 de enero de 2027.",
            Procedimiento = "Comparación directa en baño de bloque seco, en tres puntos de la banda de conservación de folículos (2 °C, 4 °C y 8 °C). Estabilización mínima de 10 minutos en cada punto. Criterio: error absoluto igual o menor que 0,3 °C en todos los puntos. Referencia interna PC-MET-04.",
            CondicionesAmbientales = "23,1 °C · 48 % HR · baño Fluke 9142.",
            Unidad = "°C",
            Criterio = "±0,3 °C en los tres puntos de la banda 2–8 °C.",
            Observaciones = "Ejemplo de certificado conforme. El registrador sigue en la nevera de folículos, sonda en el estante medio. No hizo falta ajuste. La etiqueta de calibración queda en el equipo y el certificado sigue vigente.",
            Puntos =
            [
                Punto("Banda baja · 2 °C", 2.02, 2.11, 0.30, "°C"),
                Punto("Consigna clínica · 4 °C", 4.01, 3.94, 0.30, "°C"),
                Punto("Alarma alta · 8 °C", 7.98, 8.15, 0.30, "°C")
            ]
        },
        new Calibracion
        {
            Id = "CAL-2026-004",
            EquipoId = "EQ-009",
            Fecha = new DateTime(2026, 9, 2),
            ProximaFecha = new DateTime(2026, 9, 2),
            Tecnico = "Metrología interna",
            Patron = "Tacómetro óptico, certificado TAC-2026-007.",
            CertificadoPatron = "TAC-2026-007 · vigente hasta el 4 de mayo de 2027.",
            Procedimiento = "Comparación de la velocidad indicada contra tacómetro óptico, sin carga, en las dos consignas de PRP. Criterio: ±2 % de la velocidad programada. Referencia interna PC-MET-07.",
            CondicionesAmbientales = "22,4 °C · 51 % HR · rotor vacío y tapa cerrada.",
            Unidad = "rpm",
            Criterio = "±2 % de la consigna (30 rpm a 1 500 rpm y 70 rpm a 3 500 rpm).",
            Observaciones = "Ejemplo no conforme. El punto de 3 500 rpm supera la tolerancia. La centrífuga se retiró de uso clínico y quedó abierta la orden MT-1044. No se colocó etiqueta de apto. Hace falta ajuste y una verificación nueva.",
            Puntos =
            [
                Punto("PRP suave · 1 500 rpm", 1500, 1518, 30, "rpm"),
                Punto("PRP estándar · 3 500 rpm", 3500, 3688, 70, "rpm")
            ]
        },
        new Calibracion
        {
            Id = "CAL-2025-041",
            EquipoId = "EQ-008",
            Fecha = new DateTime(2025, 9, 28),
            ProximaFecha = new DateTime(2026, 9, 28),
            Tecnico = "Metrología interna",
            Patron = "Juego de masas clase E2.",
            CertificadoPatron = "MAS-E2-2025-019 · vigente hasta el 19 de noviembre de 2026.",
            Procedimiento = "Comparación en tres cargas del rango de preparación de soluciones. Criterio: ±0,02 g. Referencia interna PC-MET-02.",
            CondicionesAmbientales = "22,8 °C · 46 % HR · nivel de burbuja correcto.",
            Unidad = "g",
            Criterio = "±0,02 g entre 1 g y 200 g.",
            Observaciones = "Comparación conforme. El certificado está dentro del último mes de vigencia: hay que programar la siguiente calibración antes del 28 de septiembre de 2026.",
            Puntos =
            [
                Punto("Carga baja · 1 g", 1.000, 1.004, 0.02, "g"),
                Punto("Carga media · 100 g", 100.000, 99.992, 0.02, "g"),
                Punto("Carga alta · 200 g", 200.000, 200.014, 0.02, "g")
            ]
        },
        new Calibracion
        {
            Id = "CAL-2025-012",
            EquipoId = "EQ-011",
            Fecha = new DateTime(2025, 3, 18),
            ProximaFecha = new DateTime(2026, 3, 18),
            Tecnico = "Metrología interna",
            Patron = "Termómetro de resistencia Fluke 1523 con sonda 5610-9.",
            CertificadoPatron = "LAC-PT-2025-031 · sustituido por el certificado vigente CAL-2026-018.",
            Procedimiento = "Misma comparación en tres puntos de la banda 2–8 °C. Referencia interna PC-MET-04.",
            CondicionesAmbientales = "22,6 °C · 45 % HR.",
            Unidad = "°C",
            Criterio = "±0,3 °C en los tres puntos.",
            Observaciones = "Certificado anterior, conservado como historial. La vigencia actual es la de CAL-2026-018.",
            Puntos =
            [
                Punto("Banda baja · 2 °C", 2.00, 2.18, 0.30, "°C"),
                Punto("Consigna clínica · 4 °C", 4.00, 4.08, 0.30, "°C"),
                Punto("Alarma alta · 8 °C", 8.00, 7.86, 0.30, "°C")
            ]
        }
    ];

    private static Equipo Equipo(
        string id, string nombre, string marca, string modelo, string serie,
        CategoriaEquipo categoria, string ubicacion, EstadoEquipo estado, string responsable,
        bool calibracion, int intervaloMantenimiento, int intervaloCalibracion, string notas) =>
        new()
        {
            Id = id,
            Nombre = nombre,
            Marca = marca,
            Modelo = modelo,
            NumeroSerie = serie,
            Categoria = categoria,
            Ubicacion = ubicacion,
            Estado = estado,
            Responsable = responsable,
            RequiereCalibracion = calibracion,
            IntervaloMantenimientoDias = intervaloMantenimiento,
            IntervaloCalibracionDias = intervaloCalibracion,
            Notas = notas
        };

    private static OrdenMantenimiento Orden(
        string id, string equipoId, TipoMantenimiento tipo, DateTime programada, DateTime? ejecucion,
        string tecnico, EstadoOrden estado, string descripcion, string hallazgos) =>
        new()
        {
            Id = id,
            EquipoId = equipoId,
            Tipo = tipo,
            FechaProgramada = programada,
            FechaEjecucion = ejecucion,
            Tecnico = tecnico,
            Estado = estado,
            Descripcion = descripcion,
            Hallazgos = hallazgos
        };

    private static PuntoCalibracion Punto(string nombre, double patron, double lectura, double tolerancia, string unidad) =>
        new()
        {
            Nombre = nombre,
            ValorPatron = patron,
            Lectura = lectura,
            Tolerancia = tolerancia,
            Unidad = unidad
        };
}
