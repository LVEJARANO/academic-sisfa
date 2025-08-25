using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcademicoSFA.Pages.ReportesNotas
{
    public class NotasGeneralModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly GrupoAcademicoRepository _grupoRepository;
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly NotasRepository _notasRepository;

        public NotasGeneralModel(
            SfaDbContext context,
            GrupoAcademicoRepository grupoRepository,
            PeriodoAcademicoRepository periodoAcademicoRepository,
            NotasRepository notasRepository)
        {
            _context = context;
            _grupoRepository = grupoRepository;
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _notasRepository = notasRepository;
        }

        // Propiedades para la vista
        public List<GrupoNotasViewModel> GruposEstudiantes { get; set; } = new List<GrupoNotasViewModel>();
        public SelectList PeriodosAcademicos { get; set; }
        public string MensajeError { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalGrupos { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PeriodoId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // 1. Cargar periodos académicos activos
                var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
                if (periodos != null && periodos.Any())
                {
                    // Obtenemos el ID del primer periodo activo
                    int periodoActivoId = periodos[0].Id;

                    // Obtenemos los periodos académicos asociados a ese periodo
                    var periodosAcademicos = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(periodoActivoId);

                    // Crear SelectList para los periodos académicos
                    var periodosSelectList = periodosAcademicos.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"Periodo {p.NumeroPeriodo} - {p.Nombre}"
                    }).ToList();

                    PeriodosAcademicos = new SelectList(periodosSelectList, "Value", "Text");

                    // Si no hay un periodo seleccionado, usar el primero
                    if (!PeriodoId.HasValue && periodosAcademicos.Any())
                    {
                        PeriodoId = periodosAcademicos.First().Id;
                    }
                }

                // 2. Si hay un periodo seleccionado, cargar las notas de todos los grupos
                if (PeriodoId.HasValue)
                {
                    await CargarNotasGenerales(PeriodoId.Value);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar los datos: {ex.Message}";
            }

            return Page();
        }

        private async Task CargarNotasGenerales(int idPeriodo)
        {
            // 1. Obtener todos los grupos académicos
            var grupos = await _grupoRepository.ObtenerGruposAsync();

            if (grupos == null || !grupos.Any())
            {
                MensajeError = "No se encontraron grupos académicos.";
                return;
            }

            GruposEstudiantes.Clear();
            TotalEstudiantes = 0;

            // 2. Para cada grupo, obtener sus estudiantes y sus notas
            foreach (var grupo in grupos)
            {
                var grupoViewModel = new GrupoNotasViewModel
                {
                    IdGrupo = grupo.Id,
                    NombreGrupo = $"{grupo.Grado.NomGrado} - Grupo {grupo.NomGrupo}"
                };

                // Obtener matrículas del grupo
                var matriculas = await _context.Matriculas
                    .Where(m => m.IdGrupoAcad == grupo.Id && m.Activo.ToLower() == "si")
                    .ToListAsync();

                if (matriculas.Any())
                {
                    // Para cada matrícula, obtener el alumno y sus notas
                    foreach (var matricula in matriculas)
                    {
                        // Obtener datos del alumno
                        var alumno = await _context.Alumnos
                            .Include(a => a.Participante)
                            .FirstOrDefaultAsync(a => a.Codigo == matricula.Codigo);

                        if (alumno == null) continue;

                        // Crear el modelo de estudiante
                        var estudianteViewModel = new EstudianteNotasGeneralViewModel
                        {
                            IdMatricula = matricula.IdMatricula,
                            Codigo = matricula.Codigo,
                            NombreCompleto = $"{alumno.Participante.Nombre} {alumno.Participante.Apellido}",
                            Materias = new List<MateriaNotasViewModel>()
                        };

                        // Buscar las notas del estudiante
                        var notasEstudiante = await _notasRepository.ObtenerNotasPorMatricula(matricula.IdMatricula);

                        // Filtrar por periodo académico
                        notasEstudiante = notasEstudiante.Where(n => n.IdPeriodoAcademico == idPeriodo).ToList();

                        if (notasEstudiante.Any())
                        {
                            // Agrupar notas por materia
                            var notasPorMateria = notasEstudiante
                                .GroupBy(n => new { n.IdMateria, n.Materia.NomMateria })
                                .ToList();

                            // Procesar cada materia
                            foreach (var materiaNota in notasPorMateria)
                            {
                                var materiaViewModel = new MateriaNotasViewModel
                                {
                                    IdMateria = materiaNota.Key.IdMateria,
                                    NombreMateria = materiaNota.Key.NomMateria,
                                    Notas = new List<NotaGeneralViewModel>()
                                };

                                // Agregar todas las notas de la materia
                                foreach (var nota in materiaNota)
                                {
                                    materiaViewModel.Notas.Add(new NotaGeneralViewModel
                                    {
                                        IdNota = nota.IdNota,
                                        NotaSaber = nota.NotaSaber,
                                        NotaHacer = nota.NotaHacer,
                                        NotaSer = nota.NotaSer,
                                        Observacion = nota.Observacion
                                    });
                                }

                                estudianteViewModel.Materias.Add(materiaViewModel);
                            }
                        }

                        // Ordenar materias alfabéticamente
                        estudianteViewModel.Materias = estudianteViewModel.Materias
                            .OrderBy(m => m.NombreMateria)
                            .ToList();

                        // Agregar el estudiante al grupo
                        grupoViewModel.Estudiantes.Add(estudianteViewModel);
                    }

                    // Ordenar estudiantes alfabéticamente
                    grupoViewModel.Estudiantes = grupoViewModel.Estudiantes
                        .OrderBy(e => e.NombreCompleto)
                        .ToList();
                }

                GruposEstudiantes.Add(grupoViewModel);
                TotalEstudiantes += grupoViewModel.Estudiantes.Count;
            }

            // Ordenar grupos por grado
            GruposEstudiantes = GruposEstudiantes
                .OrderBy(g => g.NombreGrupo)
                .ToList();

            TotalGrupos = GruposEstudiantes.Count(g => g.Estudiantes.Any());
        }
    }

    // ViewModels con nombres diferentes para evitar ambigüedades
    public class GrupoNotasViewModel
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public List<EstudianteNotasGeneralViewModel> Estudiantes { get; set; } = new List<EstudianteNotasGeneralViewModel>();
        public int TotalEstudiantes => Estudiantes.Count;

        // Propiedad para compatibilidad con código existente
        public decimal PromedioGrupo
        {
            get
            {
                if (!Estudiantes.Any()) return 0;
                return Estudiantes.Average(e => e.PromedioGeneral);
            }
        }
    }

    public class EstudianteNotasGeneralViewModel
    {
        public int IdMatricula { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public List<MateriaNotasViewModel> Materias { get; set; } = new List<MateriaNotasViewModel>();

        // Propiedad para compatibilidad con código existente
        public decimal PromedioGeneral
        {
            get
            {
                if (!Materias.Any()) return 0;
                return Materias.Average(m => m.PromedioFinal);
            }
        }
    }

    public class MateriaNotasViewModel
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; } = string.Empty;
        public List<NotaGeneralViewModel> Notas { get; set; } = new List<NotaGeneralViewModel>();

        // Propiedades para compatibilidad con código existente
        public decimal PromedioSaber
        {
            get
            {
                return Notas
                    .Where(n => n.NotaSaber.HasValue)
                    .Select(n => n.NotaSaber.Value)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }

        public decimal PromedioHacer
        {
            get
            {
                return Notas
                    .Where(n => n.NotaHacer.HasValue)
                    .Select(n => n.NotaHacer.Value)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }

        public decimal PromedioSer
        {
            get
            {
                return Notas
                    .Where(n => n.NotaSer.HasValue)
                    .Select(n => n.NotaSer.Value)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }

        public decimal PromedioFinal
        {
            get
            {
                decimal saber = PromedioSaber * 0.3m;
                decimal hacer = PromedioHacer * 0.4m;
                decimal ser = PromedioSer * 0.3m;
                return saber + hacer + ser;
            }
        }
    }

    public class NotaGeneralViewModel
    {
        public int IdNota { get; set; }
        public decimal? NotaSaber { get; set; }
        public decimal? NotaHacer { get; set; }
        public decimal? NotaSer { get; set; }
        public string? Observacion { get; set; }

        // Propiedad para compatibilidad con código existente
        public decimal NotaFinal
        {
            get
            {
                decimal saber = NotaSaber.GetValueOrDefault() * 0.3m;
                decimal hacer = NotaHacer.GetValueOrDefault() * 0.4m;
                decimal ser = NotaSer.GetValueOrDefault() * 0.3m;
                return saber + hacer + ser;
            }
        }
    }
}