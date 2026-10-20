using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MantenimientoCapilar.Modelos;
using MantenimientoCapilar.Presentacion;
using MantenimientoCapilar.Servicios;

namespace MantenimientoCapilar.Pages;

public sealed partial class InicioPage : Page
{
    public InicioPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Repositorio.Instancia.Cambio += Cargar;
        Cargar();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        Repositorio.Instancia.Cambio -= Cargar;
    }

    private void Cargar()
    {
        var repo = Repositorio.Instancia;
        var operativos = repo.Equipos.Count(eq => eq.Estado == EstadoEquipo.Operativo);
        var enTaller = repo.Equipos.Count(eq => eq.Estado == EstadoEquipo.EnMantenimiento);
        var vencidos = repo.ContarMantenimientosVencidos();
        var enCurso = repo.Mantenimientos.Count(m => repo.EstadoEfectivo(m) == EstadoOrden.EnCurso);
        var alertas = repo.ContarCalibracionesEnAlerta();
        var semana = OrdenesDeLaSemana(repo).ToList();

        CifraOperativos.Text = operativos.ToString();
        NotaOperativos.Text = enTaller == 1
            ? "1 en mantenimiento"
            : $"{enTaller} en mantenimiento";
        CifraVencidos.Text = vencidos.ToString();
        NotaVencidos.Text = enCurso == 1 ? "1 orden en curso" : $"{enCurso} órdenes en curso";
        CifraCalibracion.Text = alertas.ToString();
        NotaCalibracion.Text = TextoAlertas(repo);
        CifraSemana.Text = semana.Count.ToString();
        NotaSemana.Text = semana.Count == 0
            ? "Nada programado esta semana"
            : string.Join(" · ", semana.Take(2).Select(o => repo.EquipoPorId(o.EquipoId)?.Nombre ?? o.Id));
        Subtitulo.Text = $"Hoy {Formato.Fecha(DateTime.Today)}. Extracción, implantación, esterilización y cadena de frío.";

        var partes = new List<string>();
        if (vencidos > 0)
            partes.Add(vencidos == 1 ? "1 mantenimiento vencido" : $"{vencidos} mantenimientos vencidos");
        var noConformes = ContarSituacion(repo, SituacionCertificado.NoConforme);
        var vencidas = ContarSituacion(repo, SituacionCertificado.Vencido);
        var porVencer = ContarSituacion(repo, SituacionCertificado.PorVencer);
        if (noConformes > 0)
            partes.Add(noConformes == 1 ? "1 calibración no conforme" : $"{noConformes} calibraciones no conformes");
        if (vencidas > 0)
            partes.Add(vencidas == 1 ? "1 certificado vencido" : $"{vencidas} certificados vencidos");
        if (porVencer > 0)
            partes.Add(porVencer == 1 ? "1 calibración por vencer" : $"{porVencer} calibraciones por vencer");

        if (partes.Count == 0)
        {
            Aviso.Severity = InfoBarSeverity.Success;
            Aviso.Title = "Sin alertas";
            Aviso.Message = "No hay mantenimientos vencidos ni calibraciones pendientes.";
        }
        else
        {
            Aviso.Severity = noConformes > 0 || vencidos > 0 || vencidas > 0
                ? InfoBarSeverity.Error
                : InfoBarSeverity.Warning;
            Aviso.Title = "Requiere seguimiento";
            Aviso.Message = string.Join(". ", partes) + ".";
        }

        Aviso.IsOpen = true;

        ListaOrdenes.ItemsSource = repo.Mantenimientos
            .Where(m => repo.EstadoEfectivo(m) != EstadoOrden.Completado)
            .OrderBy(m => m.FechaProgramada)
            .Select(m => FilaOrden(repo, m))
            .ToList();

        ListaCalibraciones.ItemsSource = repo.Equipos
            .Where(eq => eq.RequiereCalibracion)
            .Select(eq => repo.UltimaCalibracion(eq.Id))
            .OfType<Calibracion>()
            .OrderBy(c => Prioridad(repo.Situacion(c)))
            .ThenBy(c => c.ProximaFecha)
            .Select(c => FilaCalibracion(repo, c))
            .ToList();
    }

    private static IEnumerable<OrdenMantenimiento> OrdenesDeLaSemana(Repositorio repo) =>
        repo.Mantenimientos.Where(m =>
        {
            var estado = repo.EstadoEfectivo(m);
            if (estado != EstadoOrden.Programado)
                return false;
            var dias = (m.FechaProgramada.Date - DateTime.Today).Days;
            return dias is >= 0 and <= 7;
        });

    private static int ContarSituacion(Repositorio repo, SituacionCertificado situacion) =>
        repo.Equipos.Count(eq =>
            eq.RequiereCalibracion
            && repo.UltimaCalibracion(eq.Id) is Calibracion cal
            && repo.Situacion(cal) == situacion);

    private static string TextoAlertas(Repositorio repo)
    {
        var noConformes = ContarSituacion(repo, SituacionCertificado.NoConforme);
        var porVencer = ContarSituacion(repo, SituacionCertificado.PorVencer);
        if (noConformes == 0 && porVencer == 0)
            return "Certificados al día";
        var partes = new List<string>();
        if (noConformes > 0)
            partes.Add(noConformes == 1 ? "1 no conforme" : $"{noConformes} no conformes");
        if (porVencer > 0)
            partes.Add(porVencer == 1 ? "1 por vencer" : $"{porVencer} por vencer");
        return string.Join(" · ", partes);
    }

    private static int Prioridad(SituacionCertificado situacion) => situacion switch
    {
        SituacionCertificado.NoConforme => 0,
        SituacionCertificado.Vencido => 1,
        SituacionCertificado.PorVencer => 2,
        SituacionCertificado.Vigente => 3,
        _ => 4
    };

    private static FilaLista FilaOrden(Repositorio repo, OrdenMantenimiento orden)
    {
        var estado = repo.EstadoEfectivo(orden);
        var equipo = repo.EquipoPorId(orden.EquipoId);
        return new FilaLista
        {
            Id = orden.Id,
            Titulo = equipo?.Nombre ?? orden.EquipoId,
            Detalle = $"{orden.Id} · {Formato.Tipo(orden.Tipo)} · {Formato.FechaCorta(orden.FechaProgramada)} · {Formato.Plazo(orden.FechaProgramada)}",
            Estado = Formato.Estado(estado),
            ColorEstado = Apariencia.De(estado)
        };
    }

    private static FilaLista FilaCalibracion(Repositorio repo, Calibracion calibracion)
    {
        var situacion = repo.Situacion(calibracion);
        var equipo = repo.EquipoPorId(calibracion.EquipoId);
        return new FilaLista
        {
            Id = calibracion.Id,
            Titulo = equipo?.Nombre ?? calibracion.EquipoId,
            Detalle = $"{calibracion.Id} · {Formato.FechaCorta(calibracion.Fecha)} · próxima {Formato.FechaCorta(calibracion.ProximaFecha)}",
            Estado = Formato.Situacion(situacion),
            ColorEstado = Apariencia.De(situacion)
        };
    }

    private void Operativos_Click(object sender, RoutedEventArgs e) => MainWindow.Instancia?.Navegar("equipos");

    private void Vencidos_Click(object sender, RoutedEventArgs e) => MainWindow.Instancia?.Navegar("mantenimiento");

    private void Semana_Click(object sender, RoutedEventArgs e) => MainWindow.Instancia?.Navegar("mantenimiento");

    private void Calibracion_Click(object sender, RoutedEventArgs e) =>
        MainWindow.Instancia?.Navegar("calibracion", Repositorio.IdEjemploCalibracion);

    private void Ordenes_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is FilaLista fila)
            MainWindow.Instancia?.Navegar("mantenimiento", fila.Id);
    }

    private void Calibraciones_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is FilaLista fila)
            MainWindow.Instancia?.Navegar("calibracion", fila.Id);
    }
}
