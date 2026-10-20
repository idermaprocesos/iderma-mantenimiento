using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MantenimientoCapilar.Servicios;

public static class ExportacionPdf
{
    private const string Azul = "#2F4373";
    private const string Dorado = "#B59451";
    private const string Tinta = "#243044";
    private const string AzulSuave = "#E7EDF6";
    private const string DoradoSuave = "#F8F4EA";
    private const string DoradoLinea = "#E4D3A8";
    private const string NoConforme = "#8C2F2F";
    private const string NoConformeFondo = "#F8E8E4";

    static ExportacionPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static void Certificado(Calibracion calibracion, string ruta) =>
        Documento(ruta, columna => ComponerCertificado(columna, calibracion));

    public static void Certificados(IReadOnlyList<Calibracion> calibraciones, string ruta)
    {
        var lista = calibraciones.OrderByDescending(c => c.Fecha).ThenByDescending(c => c.Id).ToList();
        Documento(ruta, columna =>
        {
            columna.Item().Text("Calibraciones").FontSize(20).Bold().FontColor(Azul);
            columna.Item().PaddingTop(4).Text($"{lista.Count} certificados · {Formato.Fecha(DateTime.Today)}").FontColor(Dorado);
            columna.Item().PaddingTop(14).Element(contenedor => TablaCalibraciones(contenedor, lista));

            foreach (var calibracion in lista)
            {
                columna.Item().PageBreak();
                ComponerCertificado(columna, calibracion);
            }
        });
    }

    public static void Orden(OrdenMantenimiento orden, string ruta) =>
        Documento(ruta, columna => ComponerOrden(columna, orden));

    public static void Ordenes(IReadOnlyList<OrdenMantenimiento> ordenes, string ruta)
    {
        var lista = ordenes.OrderBy(o => o.FechaProgramada).ToList();
        Documento(ruta, columna =>
        {
            columna.Item().Text("Mantenimiento").FontSize(20).Bold().FontColor(Azul);
            columna.Item().PaddingTop(4).Text($"{lista.Count} órdenes · {Formato.Fecha(DateTime.Today)}").FontColor(Dorado);
            columna.Item().PaddingTop(14).Element(contenedor => TablaOrdenes(contenedor, lista));

            foreach (var orden in lista)
            {
                columna.Item().PageBreak();
                ComponerOrden(columna, orden);
            }
        });
    }

    private static void Documento(string ruta, Action<ColumnDescriptor> componer)
    {
        Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(36);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(10).FontColor(Tinta));

                pagina.Header().PaddingBottom(10).Column(cabecera =>
                {
                    var logo = RutaLogo();
                    if (logo is not null)
                        cabecera.Item().AlignCenter().Height(58).Image(logo).FitArea();
                    else
                        cabecera.Item().AlignCenter().Text("IDERMA CAPILAR").FontSize(18).Bold().FontColor(Azul);

                    cabecera.Item().PaddingTop(6).LineHorizontal(2).LineColor(Dorado);
                    cabecera.Item().PaddingTop(2).LineHorizontal(0.6f).LineColor(Azul);
                });

                pagina.Content().PaddingTop(14).Column(componer);

                pagina.Footer().AlignRight().DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(Azul)).Text(texto =>
                {
                    texto.Span("Iderma Capilar  ·  Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf(ruta);
    }

    private static void ComponerCertificado(ColumnDescriptor columna, Calibracion calibracion)
    {
        var repo = Repositorio.Instancia;
        var equipo = repo.EquipoPorId(calibracion.EquipoId);
        var situacion = repo.Situacion(calibracion);

        columna.Item().Text("Certificado de calibración").FontSize(18).Bold().FontColor(Azul);
        columna.Item().PaddingTop(2).Text(calibracion.Id).FontSize(12).FontColor(Dorado);
        columna.Item().PaddingTop(10).Text(equipo?.Nombre ?? calibracion.EquipoId).FontSize(14).SemiBold().FontColor(Azul);
        if (equipo is not null)
        {
            columna.Item().Text($"{equipo.Id} · {equipo.Marca} {equipo.Modelo} · {equipo.Ubicacion}").FontColor(Azul);
        }

        columna.Item().PaddingTop(10).Text($"{Formato.Resultado(calibracion.Resultado)} · {Formato.Situacion(situacion)}")
            .Bold()
            .FontColor(calibracion.Resultado == ResultadoCalibracion.Conforme ? Azul : NoConforme);

        columna.Item().PaddingTop(12).Element(contenedor => Pares(contenedor,
        [
            ("Fecha", Formato.Fecha(calibracion.Fecha)),
            ("Próxima calibración", $"{Formato.Fecha(calibracion.ProximaFecha)} · {Formato.Plazo(calibracion.ProximaFecha)}"),
            ("Responsable", calibracion.Tecnico),
            ("Criterio", calibracion.Criterio),
            ("Condiciones", Texto(calibracion.CondicionesAmbientales)),
            ("Unidad", calibracion.Unidad)
        ]));

        columna.Item().PaddingTop(12).Text("Patrón").SemiBold().FontColor(Azul);
        columna.Item().Text(Texto(calibracion.Patron));
        columna.Item().Text(Texto(calibracion.CertificadoPatron)).FontColor(Dorado);

        columna.Item().PaddingTop(12).Text("Puntos de comparación").SemiBold().FontColor(Azul);
        columna.Item().PaddingTop(4).Element(contenedor => TablaPuntos(contenedor, calibracion));
        columna.Item().PaddingTop(4).Text("Error = lectura del instrumento menos el valor del patrón.").FontSize(8).FontColor(Azul);

        columna.Item().PaddingTop(12).Text("Procedimiento").SemiBold().FontColor(Azul);
        columna.Item().Text(Texto(calibracion.Procedimiento));
        columna.Item().PaddingTop(8).Text("Observaciones").SemiBold().FontColor(Azul);
        columna.Item().Text(Texto(calibracion.Observaciones));
    }

    private static void ComponerOrden(ColumnDescriptor columna, OrdenMantenimiento orden)
    {
        var repo = Repositorio.Instancia;
        var equipo = repo.EquipoPorId(orden.EquipoId);
        var estado = repo.EstadoEfectivo(orden);

        columna.Item().Text("Orden de mantenimiento").FontSize(18).Bold().FontColor(Azul);
        columna.Item().PaddingTop(2).Text(orden.Id).FontSize(12).FontColor(Dorado);
        columna.Item().PaddingTop(10).Text(equipo?.Nombre ?? orden.EquipoId).FontSize(14).SemiBold().FontColor(Azul);
        if (equipo is not null)
            columna.Item().Text($"{equipo.Id} · {equipo.Ubicacion}").FontColor(Azul);

        columna.Item().PaddingTop(10).Text(Formato.Estado(estado)).Bold().FontColor(Azul);
        columna.Item().PaddingTop(12).Element(contenedor => Pares(contenedor,
        [
            ("Tipo", Formato.Tipo(orden.Tipo)),
            ("Responsable", orden.Tecnico),
            ("Fecha programada", $"{Formato.Fecha(orden.FechaProgramada)} · {Formato.Plazo(orden.FechaProgramada)}"),
            ("Ejecución", orden.FechaEjecucion is DateTime hecha ? Formato.Fecha(hecha) : "Pendiente")
        ]));

        columna.Item().PaddingTop(12).Text("Trabajo").SemiBold().FontColor(Azul);
        columna.Item().Text(Texto(orden.Descripcion));
        columna.Item().PaddingTop(8).Text("Cierre").SemiBold().FontColor(Azul);
        columna.Item().Text(Texto(orden.Hallazgos));
    }

    private static void Pares(IContainer contenedor, IReadOnlyList<(string Etiqueta, string Valor)> pares)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn();
                columnas.RelativeColumn();
            });

            for (var i = 0; i < pares.Count; i++)
            {
                var (etiqueta, valor) = pares[i];
                var fondoEtiqueta = i % 2 == 0 ? AzulSuave : DoradoSuave;
                var fondoValor = i % 2 == 0 ? "#FFFFFF" : DoradoSuave;
                tabla.Cell().Background(fondoEtiqueta).Padding(4).Text(etiqueta).FontColor(Azul);
                tabla.Cell().Background(fondoValor).Padding(4).Text(Texto(valor));
            }
        });
    }

    private static void TablaPuntos(IContainer contenedor, Calibracion calibracion)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.RelativeColumn(2.2f);
                columnas.RelativeColumn();
                columnas.RelativeColumn();
                columnas.RelativeColumn();
                columnas.RelativeColumn();
                columnas.RelativeColumn();
            });

            tabla.Header(encabezado =>
            {
                Encabezado(encabezado.Cell(), "Punto", false);
                Encabezado(encabezado.Cell(), "Patrón", true);
                Encabezado(encabezado.Cell(), "Lectura", true);
                Encabezado(encabezado.Cell(), "Error", true);
                Encabezado(encabezado.Cell(), "Tolerancia", true);
                Encabezado(encabezado.Cell(), "Dictamen", false);
            });

            foreach (var punto in calibracion.Puntos)
            {
                var fondo = punto.Conforme ? ("#FFFFFF") : NoConformeFondo;
                var dictamen = punto.Conforme ? "Conforme" : "No conforme";
                Celda(tabla.Cell(), punto.Nombre, fondo, false);
                Celda(tabla.Cell(), Formato.Medida(punto.ValorPatron, punto.Unidad), fondo, true);
                Celda(tabla.Cell(), Formato.Medida(punto.Lectura, punto.Unidad), fondo, true);
                Celda(tabla.Cell(), Formato.Error(punto.Error, punto.Unidad), fondo, true);
                Celda(tabla.Cell(), Formato.Tolerancia(punto.Tolerancia, punto.Unidad), fondo, true);
                tabla.Cell().Background(fondo).BorderBottom(0.4f).BorderColor(DoradoLinea).Padding(4)
                    .AlignLeft()
                    .Text(dictamen)
                    .FontColor(punto.Conforme ? Azul : NoConforme);
            }
        });
    }

    private static void TablaCalibraciones(IContainer contenedor, IReadOnlyList<Calibracion> calibraciones)
    {
        var repo = Repositorio.Instancia;
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.ConstantColumn(92);
                columnas.RelativeColumn(2);
                columnas.RelativeColumn();
                columnas.RelativeColumn();
                columnas.RelativeColumn();
            });

            tabla.Header(encabezado =>
            {
                Encabezado(encabezado.Cell(), "Certificado", false);
                Encabezado(encabezado.Cell(), "Equipo", false);
                Encabezado(encabezado.Cell(), "Fecha", false);
                Encabezado(encabezado.Cell(), "Próxima", false);
                Encabezado(encabezado.Cell(), "Situación", false);
            });

            for (var i = 0; i < calibraciones.Count; i++)
            {
                var calibracion = calibraciones[i];
                var equipo = repo.EquipoPorId(calibracion.EquipoId);
                var fondo = i % 2 == 0 ? "#FFFFFF" : DoradoSuave;
                Celda(tabla.Cell(), calibracion.Id, fondo, false);
                Celda(tabla.Cell(), equipo?.Nombre ?? calibracion.EquipoId, fondo, false);
                Celda(tabla.Cell(), Formato.FechaCorta(calibracion.Fecha), fondo, false);
                Celda(tabla.Cell(), Formato.FechaCorta(calibracion.ProximaFecha), fondo, false);
                Celda(tabla.Cell(), Formato.Situacion(repo.Situacion(calibracion)), fondo, false);
            }
        });
    }

    private static void TablaOrdenes(IContainer contenedor, IReadOnlyList<OrdenMantenimiento> ordenes)
    {
        var repo = Repositorio.Instancia;
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.ConstantColumn(72);
                columnas.RelativeColumn(2);
                columnas.RelativeColumn();
                columnas.RelativeColumn();
                columnas.RelativeColumn();
            });

            tabla.Header(encabezado =>
            {
                Encabezado(encabezado.Cell(), "Orden", false);
                Encabezado(encabezado.Cell(), "Equipo", false);
                Encabezado(encabezado.Cell(), "Tipo", false);
                Encabezado(encabezado.Cell(), "Fecha", false);
                Encabezado(encabezado.Cell(), "Estado", false);
            });

            for (var i = 0; i < ordenes.Count; i++)
            {
                var orden = ordenes[i];
                var equipo = repo.EquipoPorId(orden.EquipoId);
                var fondo = i % 2 == 0 ? "#FFFFFF" : DoradoSuave;
                Celda(tabla.Cell(), orden.Id, fondo, false);
                Celda(tabla.Cell(), equipo?.Nombre ?? orden.EquipoId, fondo, false);
                Celda(tabla.Cell(), Formato.Tipo(orden.Tipo), fondo, false);
                Celda(tabla.Cell(), Formato.FechaCorta(orden.FechaProgramada), fondo, false);
                Celda(tabla.Cell(), Formato.Estado(repo.EstadoEfectivo(orden)), fondo, false);
            }
        });
    }

    private static void Encabezado(IContainer celda, string texto, bool derecha)
    {
        var bloque = celda.Background(Azul).Padding(4);
        if (derecha)
            bloque = bloque.AlignRight();

        bloque.Text(texto).SemiBold().FontSize(8).FontColor("#FFFFFF");
    }

    private static void Celda(IContainer celda, string texto, string fondo, bool derecha)
    {
        var bloque = celda.Background(fondo).BorderBottom(0.4f).BorderColor(DoradoLinea).Padding(4);
        if (derecha)
            bloque = bloque.AlignRight();

        bloque.Text(texto).FontColor(Tinta);
    }

    private static string? RutaLogo()
    {
        var juntoAlPrograma = Path.Combine(AppContext.BaseDirectory, "img", "IDERMA LOGO.jpg");
        if (File.Exists(juntoAlPrograma))
            return juntoAlPrograma;

        var enProyecto = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "img", "IDERMA LOGO.jpg"));
        return File.Exists(enProyecto) ? enProyecto : null;
    }

    private static string Texto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? "—" : valor.Trim();
}
