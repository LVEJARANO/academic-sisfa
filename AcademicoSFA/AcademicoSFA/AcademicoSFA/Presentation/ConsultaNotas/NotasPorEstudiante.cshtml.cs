using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicoSFA.Pages.ReportesNotas
{
    public class NotasPorCodigoEstudianteModel : PageModel
    {
        private readonly IPeriodoRepository _periodoAcademicoRepository;
        private readonly INotasRepository _notasRepository;
        private readonly IAlumnoRepository _alumnoRepository;
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly INotyfService _servicioNotificacion;

        public NotasPorCodigoEstudianteModel(
            SfaDbContext context,
            IPeriodoRepository periodoAcademicoRepository,
            INotasRepository notasRepository,
            IMatriculaRepository matriculaRepository,
            IAlumnoRepository alumnoRepository,
            INotyfService servicioNotificacion)
        {
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _notasRepository = notasRepository;
            _alumnoRepository = alumnoRepository;
            _matriculaRepository = matriculaRepository;
            _servicioNotificacion = servicioNotificacion;

            // Inicializar listas
            PeriodosAcademicos = new List<SelectListItem>();
            MateriasConNotas = new List<MateriaNotasDetalladasDTO>();
        }

        // Propiedades para la vista
        public string NombreEstudiante { get; set; }
        public string CodigoEstudiante { get; set; }
        public string GradoEstudiante { get; set; }
        public List<MateriaNotasDetalladasDTO> MateriasConNotas { get; set; }
        public List<SelectListItem> PeriodosAcademicos { get; set; }
        public string MensajeError { get; set; }
        public string MensajeExito { get; set; }
        public int? IdMatricula { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CodigoBusqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PeriodoId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // 1. Cargar periodos académicos
                await CargarPeriodosAcademicos();
                // 2. Si hay un código de búsqueda, buscar al estudiante
                if (!string.IsNullOrEmpty(CodigoBusqueda) && PeriodoId.HasValue)
                {
                    var estudiante = await  _alumnoRepository.ObtenerAlumnoConParticipantePorCodigoAsync(CodigoBusqueda);
                    if (estudiante != null)
                    {
                        NombreEstudiante = $"{estudiante.Participante.Nombre} {estudiante.Participante.Apellido}";
                        CodigoEstudiante = estudiante.Codigo;
                        var matricula = await _matriculaRepository.ObtenerMatriculaConGradoPorCodigoEstudiante(estudiante.Codigo);
                        if (matricula != null)
                        {
                            GradoEstudiante = matricula.GruposAcad.Grado.NomGrado;
                            IdMatricula = matricula.IdMatricula;
                            // Cargar notas detalladas
                            await CargarNotasDetalladas(matricula.IdMatricula, PeriodoId.Value);
                        }
                        else
                        {
                            MensajeError = "No se encontró información de matrícula para el estudiante.";
                            _servicioNotificacion.Error("No se encontró información de matrícula para el estudiante.");
                        }
                    }
                    else
                    {
                        MensajeError = $"No se encontró ningún estudiante con el código '{CodigoBusqueda}'.";
                        _servicioNotificacion.Error($"No se encontró ningún estudiante con el código '{CodigoBusqueda}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar los datos: {ex.Message}";
                _servicioNotificacion.Error($"Error al cargar los datos: {ex.Message}");
            }

            return Page();
        }

        private async Task CargarPeriodosAcademicos()
        {
            var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
            if (periodos != null && periodos.Any())
            {
                // Obtener el ID del primer periodo activo
                int periodoActivoId = periodos[0].Id;
                // Obtener los periodos académicos asociados a ese periodo
                var periodosAcademicos = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(periodoActivoId);
                // Crear la lista para el dropdown
                var items = new List<SelectListItem>();
                foreach (var periodoAcad in periodosAcademicos)
                {
                    items.Add(new SelectListItem
                    {
                        Value = periodoAcad.Id.ToString(),
                        Text = $"Periodo {periodoAcad.NumeroPeriodo} - {periodoAcad.Nombre}",
                        Selected = PeriodoId.HasValue && PeriodoId.Value == periodoAcad.Id
                    });
                }
                // Si no hay un periodo seleccionado, seleccionar el primero
                if (!PeriodoId.HasValue && items.Any())
                {
                    PeriodoId = int.Parse(items.First().Value);
                    items.First().Selected = true;
                }
                PeriodosAcademicos = items;
            }
        }
        private async Task CargarNotasDetalladas(int idMatricula, int idPeriodo)
        {
            try
            {
                // Obtener todas las notas del estudiante usando el repositorio existente
                var notasCompletas = await _notasRepository.ObtenerNotasPorMatricula(idMatricula);
                // Filtrar por el periodo seleccionado 
                var notasPeriodo = notasCompletas.Where(n => n.IdPeriodoAcademico == idPeriodo).ToList();
                if (notasPeriodo == null || !notasPeriodo.Any())
                {
                    return;
                }
                // Agrupar por materia
                var notasPorMateria = notasPeriodo
                    .GroupBy(n => new { n.IdMateria, n.Materia.NomMateria })
                    .ToList();

                // Crear modelos para la vista
                foreach (var grupo in notasPorMateria)
                {
                    var materiaViewModel = new MateriaNotasDetalladasDTO
                    {
                        IdMateria = grupo.Key.IdMateria,
                        NombreMateria = grupo.Key.NomMateria,
                        Notas = grupo.Select(n => new NotaDetalladaDTO
                        {
                            IdNota = n.IdNota,
                            NotaSaber = n.NotaSaber,
                            NotaHacer = n.NotaHacer,
                            NotaSer = n.NotaSer,
                            Observacion = n.Observacion
                        }).OrderBy(n => n.FechaRegistro).ToList()
                    };

                    MateriasConNotas.Add(materiaViewModel);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar las notas: {ex.Message}";
                _servicioNotificacion.Error($"Error al cargar las notas: {ex.Message}");
            }
        }
    }
}