using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using AcademicoSFA.Domain.Monitoring; // ← NUEVA LÍNEA
using System.Diagnostics; // ← NUEVA LÍNEA

namespace AcademicoSFA.Pages.ConsultaNotas
{
    public class IndexModel : PageModel
    {
        private readonly IMatriculaRepository _matriculaRepository;
        private readonly IPeriodoRepository _periodoAcademicoRepository;
        private readonly INotasRepository _notasRepository;
        private readonly ISisfaMetrics _metrics; // ← NUEVA LÍNEA
        private readonly ILogger<IndexModel> _logger; // ← NUEVA LÍNEA

        public IndexModel(IMatriculaRepository matriculaRepository,
            IPeriodoRepository periodoAcademicoRepository,
            INotasRepository notasRepository,
            ISisfaMetrics metrics, // ← NUEVA LÍNEA
            ILogger<IndexModel> logger) // ← NUEVA LÍNEA
        {
            _matriculaRepository = matriculaRepository;
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _notasRepository = notasRepository;
            _metrics = metrics; // ← NUEVA LÍNEA
            _logger = logger; // ← NUEVA LÍNEA
        }

        // Propiedades para la vista (tu código existente)
        public string NombreEstudiante { get; set; }
        public List<MateriaConNotasDTO> MateriasConNotas { get; set; } = new List<MateriaConNotasDTO>();
        public SelectList PeriodosAcademicos { get; set; }
        public string MensajeError { get; set; }
        public decimal PromedioGeneral { get; set; }
        public string NumeroMatricula { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PeriodoId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

            // Registrar vista de página
            _metrics.RecordPageView("ConsultaNotas"); // ← NUEVA LÍNEA

            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Invitado";

            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToPage("/InicioSesion/Login");
            }

            try
            {
                _logger.LogInformation("Consultando notas para usuario: {UserId}", idUsuario); // ← NUEVA LÍNEA

                // Buscar la matrícula del estudiante actual
                NumeroMatricula = await _matriculaRepository.ObtenerNumeroMatriculaParticipante(int.Parse(idUsuario));
                if (NumeroMatricula == null)
                {
                    MensajeError = "No se encontró información de matrícula para el estudiante.";
                    return Page();
                }

                NombreEstudiante = User.FindFirstValue(ClaimTypes.Name) ?? "N/A";

                // Cargar los periodos académicos disponibles
                var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
                if (periodos != null && periodos.Any())
                {
                    int periodoActivoId = periodos[0].Id;
                    var periodosAcademicos = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(periodoActivoId);

                    var selectItems = new List<SelectListItem>();
                    foreach (var periodoAcad in periodosAcademicos)
                    {
                        selectItems.Add(new SelectListItem
                        {
                            Value = periodoAcad.Id.ToString(),
                            Text = $"Periodo {periodoAcad.NumeroPeriodo} - {periodoAcad.Nombre}"
                        });
                    }

                    PeriodosAcademicos = new SelectList(selectItems, "Value", "Text");

                    if (!PeriodoId.HasValue && selectItems.Any())
                    {
                        PeriodoId = int.Parse(selectItems.First().Value);
                    }

                    if (PeriodoId.HasValue)
                    {
                        await CargarNotasEstudiante(int.Parse(NumeroMatricula), PeriodoId.Value);
                    }
                }

                // ===== REGISTRAR MÉTRICAS DE CONSULTA EXITOSA =====
                var duration = stopwatch.Elapsed.TotalSeconds; // ← NUEVA LÍNEA
                _metrics.RecordConsultaNotasTime(duration); // ← NUEVA LÍNEA
                _logger.LogInformation("Consulta de notas exitosa para usuario {UserId} en {Duration}ms",
                    idUsuario, stopwatch.ElapsedMilliseconds); // ← NUEVA LÍNEA

            }
            catch (Exception ex)
            {
                // ===== REGISTRAR MÉTRICAS DE ERROR =====
                _logger.LogError(ex, "Error al cargar notas para usuario {UserId}", idUsuario); // ← NUEVA LÍNEA
                MensajeError = $"Error al cargar los periodos académicos: {ex.Message}";
            }

            return Page();
        }

        private async Task CargarNotasEstudiante(int idMatricula, int idPeriodo)
        {
            var stopwatch = Stopwatch.StartNew(); // ← NUEVA LÍNEA

            try
            {
                MateriasConNotas = await _notasRepository.ObtenerNotasAgrupadas(idMatricula, idPeriodo);

                // ===== REGISTRAR MÉTRICAS DE OPERACIÓN DE BD =====
                _metrics.RecordDatabaseOperation("obtener_notas_agrupadas", true, stopwatch.Elapsed.TotalSeconds); // ← NUEVA LÍNEA

                _logger.LogInformation("Notas cargadas exitosamente para matrícula {IdMatricula}, periodo {IdPeriodo}. {Count} materias encontradas",
                    idMatricula, idPeriodo, MateriasConNotas.Count); // ← NUEVA LÍNEA
            }
            catch (Exception ex)
            {
                // ===== REGISTRAR MÉTRICAS DE ERROR EN BD =====
                _metrics.RecordDatabaseOperation("obtener_notas_agrupadas", false, stopwatch.Elapsed.TotalSeconds); // ← NUEVA LÍNEA
                _logger.LogError(ex, "Error cargando notas para matrícula {IdMatricula}, periodo {IdPeriodo}", idMatricula, idPeriodo); // ← NUEVA LÍNEA
                throw;
            }
        }
    }
}