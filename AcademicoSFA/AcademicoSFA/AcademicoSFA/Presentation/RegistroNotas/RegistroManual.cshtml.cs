using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AcademicoSFA.Pages.RegistroNotas
{
    public class RegistroManualModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly NotasRepository _notasRepository;
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly ILogger<RegistroManualModel> _logger;

        public RegistroManualModel(
            SfaDbContext context,
            NotasRepository notasRepository,
            PeriodoAcademicoRepository periodoAcademicoRepository,
            ILogger<RegistroManualModel> logger)
        {
            _context = context;
            _notasRepository = notasRepository;
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? EstudianteId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Codigo { get; set; }

        public List<EstudianteViewModel> EstudiantesEncontrados { get; set; } = new List<EstudianteViewModel>();
        public EstudianteViewModel EstudianteSeleccionado { get; set; }
        public List<MateriaViewModel> Materias { get; set; } = new List<MateriaViewModel>();
        public List<PeriodoAcademicoViewModel> PeriodosAcademicos { get; set; } = new List<PeriodoAcademicoViewModel>();
        public bool NotificacionExito { get; set; }
        public List<NotificacionCarga> Notificaciones { get; set; } = new List<NotificacionCarga>();

        public async Task OnGetAsync()
        {
            // Cargar lista de materias
            Materias = await _context.Materias
                .Select(m => new MateriaViewModel
                {
                    Id = m.Id,
                    Nombre = m.NomMateria
                })
                .ToListAsync();

            // Cargar periodos académicos
            PeriodosAcademicos = await _context.PeriodoAcademico
                .OrderByDescending(p => p.FechaInicio)
                .Select(p => new PeriodoAcademicoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre
                })
                .ToListAsync();

            // Buscar estudiantes si hay término de búsqueda
            if (!string.IsNullOrEmpty(SearchTerm) && SearchTerm.Length >= 3)
            {
                EstudiantesEncontrados = await BuscarEstudiantes(SearchTerm);
            }

            // Cargar estudiante seleccionado si hay ID o código
            if (EstudianteId.HasValue)
            {
                EstudianteSeleccionado = await ObtenerEstudiantePorId(EstudianteId.Value);
            }
            else if (!string.IsNullOrEmpty(Codigo))
            {
                EstudianteSeleccionado = await ObtenerEstudiantePorCodigo(Codigo);
            }

            // Comprobar si hay notificación de éxito en TempData
            if (TempData["NotificacionExito"] != null)
            {
                NotificacionExito = true;
                TempData.Remove("NotificacionExito");
            }
        }

        public async Task<IActionResult> OnPostAsync(
            int idParticipante,
            string codigo,
            int idMateria,
            int idPeriodoAcademico,
            decimal? notaSer1,
            decimal? notaSer2,
            decimal? notaSer3,
            decimal? notaHacer1,
            decimal? notaHacer2,
            decimal? notaHacer3,
            decimal? notaHacer4,
            decimal? notaSaber1,
            decimal? notaSaber2,
            decimal? notaSaber3,
            decimal? notaSaber4,
            decimal? examenFinal,
            string observacion)
        {
            try
            {
                // Obtener la matrícula del estudiante
                var matricula = await _context.Matriculas
                    .FirstOrDefaultAsync(m => m.Codigo == codigo);

                if (matricula == null)
                {
                    AddToast("Error", "No se encontró matrícula para el estudiante", "error");
                    return RedirectToPage(new { EstudianteId = idParticipante, Codigo = codigo });
                }

                int notasGuardadas = 0;

                // Guardar notas de Saber Ser (Heteroevaluación)
                if (notaSer1.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        null,
                        notaSer1.Value,
                        "SABER SER - HETEROEVALUACION");
                    notasGuardadas++;
                }

                // Guardar notas de Saber Ser (Autoevaluación)
                if (notaSer2.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        null,
                        notaSer2.Value,
                        "SABER SER - AUTOEVALUACION");
                    notasGuardadas++;
                }

                // Guardar notas de Saber Ser (Coevaluación)
                if (notaSer3.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        null,
                        notaSer3.Value,
                        "SABER SER - COEVALUACION");
                    notasGuardadas++;
                }

                // Guardar notas de Saber Hacer
                if (notaHacer1.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        notaHacer1.Value,
                        null,
                        "NOTA 1 SABER HACER");
                    notasGuardadas++;
                }

                if (notaHacer2.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        notaHacer2.Value,
                        null,
                        "NOTA 2 SABER HACER");
                    notasGuardadas++;
                }

                if (notaHacer3.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        notaHacer3.Value,
                        null,
                        "NOTA 3 SABER HACER");
                    notasGuardadas++;
                }

                if (notaHacer4.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        notaHacer4.Value,
                        null,
                        "NOTA 4 SABER HACER");
                    notasGuardadas++;
                }

                // Guardar notas de Saber Saber
                if (notaSaber1.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        notaSaber1.Value,
                        null,
                        null,
                        "NOTA 15 saber saber");
                    notasGuardadas++;
                }

                if (notaSaber2.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        notaSaber2.Value,
                        null,
                        null,
                        "NOTA 16 saber saber");
                    notasGuardadas++;
                }

                if (notaSaber3.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        notaSaber3.Value,
                        null,
                        null,
                        "NOTA 17 saber saber");
                    notasGuardadas++;
                }

                if (notaSaber4.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        notaSaber4.Value,
                        null,
                        null,
                        "NOTA 18 saber saber");
                    notasGuardadas++;
                }

                // Guardar Examen Final
                if (examenFinal.HasValue)
                {
                    await _notasRepository.GuardarNotaAsync(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        null,
                        null,
                        null,
                        null);
                    notasGuardadas++;
                }


                // Guardar nota con observación general (opcional)
                if (!string.IsNullOrEmpty(observacion))
                {
                    await _notasRepository.GuardarObservacion(
                        matricula.IdMatricula,
                        idMateria,
                        idPeriodoAcademico,
                        observacion);
                }

                // Obtener nombre de la materia para el mensaje de éxito
                var nombreMateria = await _context.Materias
                    .Where(m => m.Id == idMateria)
                    .Select(m => m.NomMateria)
                    .FirstOrDefaultAsync();

                // Mostrar notificación de éxito
                AddToast("Éxito", $"Se guardaron {notasGuardadas} notas para la materia {nombreMateria}", "success");
                TempData["NotificacionExito"] = true;


                // Mantener al estudiante seleccionado
                return RedirectToPage(new { EstudianteId = idParticipante, Codigo = codigo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar notas manualmente");
                AddToast("Error", $"Error al guardar notas: {ex.Message}", "error");
                return RedirectToPage(new { EstudianteId = idParticipante, Codigo = codigo });
            }
        }

        private async Task<List<EstudianteViewModel>> BuscarEstudiantes(string termino)
        {
            // Normalizar el término de búsqueda (eliminar acentos, convertir a minúsculas)
            string terminoNormalizado = termino.ToLower().Trim();

            // Consulta optimizada para el modelo de datos actual
            var query = from p in _context.Participantes
                        join a in _context.Alumnos on p.Id equals a.IdParticipante
                        where p.Eliminado == false &&
                              (p.Documento.Contains(terminoNormalizado) ||
                               p.Nombre.ToLower().Contains(terminoNormalizado) ||
                               p.Apellido.ToLower().Contains(terminoNormalizado))
                        select new EstudianteViewModel
                        {
                            IdParticipante = p.Id,
                            Codigo = a.Codigo,
                            NombreCompleto = p.Nombre + " " + p.Apellido,
                            Documento = p.Documento,
                            Email = p.Email
                        };

            return await query.Take(10).ToListAsync();
        }

        private async Task<EstudianteViewModel> ObtenerEstudiantePorId(int idParticipante)
        {
            // Consulta por ID de participante
            var query = from p in _context.Participantes
                        join a in _context.Alumnos on p.Id equals a.IdParticipante
                        where p.Id == idParticipante
                        select new EstudianteViewModel
                        {
                            IdParticipante = p.Id,
                            Codigo = a.Codigo,
                            NombreCompleto = p.Nombre + " " + p.Apellido,
                            Documento = p.Documento,
                            Email = p.Email
                        };

            return await query.FirstOrDefaultAsync();
        }

        private async Task<EstudianteViewModel> ObtenerEstudiantePorCodigo(string codigo)
        {
            // Consulta por código de alumno
            var query = from p in _context.Participantes
                        join a in _context.Alumnos on p.Id equals a.IdParticipante
                        where a.Codigo == codigo
                        select new EstudianteViewModel
                        {
                            IdParticipante = p.Id,
                            Codigo = a.Codigo,
                            NombreCompleto = p.Nombre + " " + p.Apellido,
                            Documento = p.Documento,
                            Email = p.Email
                        };

            return await query.FirstOrDefaultAsync();
        }

        private void AddToast(string title, string message, string type = "success")
        {
            // Si no hay múltiples toasts, inicializar la lista
            if (TempData["MultipleToasts"] == null)
            {
                TempData["MultipleToasts"] = JsonSerializer.Serialize(new List<ToastNotification>());
            }

            // Obtener la lista actual
            var toastsJson = TempData["MultipleToasts"].ToString();
            var toasts = JsonSerializer.Deserialize<List<ToastNotification>>(toastsJson);

            // Añadir la nueva notificación
            toasts.Add(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = type == "error" || type == "danger" ? 8000 : 5000 // Más tiempo para errores
            });

            // Actualizar TempData
            TempData["MultipleToasts"] = JsonSerializer.Serialize(toasts);
        }
    }

    // Clases de vista solo para esta página
    public class EstudianteViewModel
    {
        public int IdParticipante { get; set; }
        public string Codigo { get; set; }
        public string NombreCompleto { get; set; }
        public string Documento { get; set; }
        public string Email { get; set; }
    }

    public class MateriaViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class PeriodoAcademicoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}