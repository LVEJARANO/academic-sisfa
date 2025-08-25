using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
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
    public class NotasPorMateriaModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IGrupoAcademicoRepository _grupoRepository;
        private readonly IPeriodoRepository _periodoAcademicoRepository;
        private readonly INotasRepository _notasRepository;

        public NotasPorMateriaModel(
            SfaDbContext context,
            IGrupoAcademicoRepository grupoRepository,
            IPeriodoRepository periodoAcademicoRepository,
            INotasRepository notasRepository)
        {
            _context = context;
            _grupoRepository = grupoRepository;
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _notasRepository = notasRepository;

            // Inicializa las colecciones vacías
            EstudiantesNotas = new List<EstudianteNotasMateriaDTO>();
        }

        // Propiedades para la vista
        public string NombreMateria { get; set; }
        public string NombreGrado { get; set; }
        public string NombreGrupo { get; set; }
        public string Grados { get; set; }
        public List<EstudianteNotasMateriaDTO> EstudiantesNotas { get; set; } = new List<EstudianteNotasMateriaDTO>();
        public string MensajeError { get; set; }
        public decimal PromedioGeneralCurso { get; set; }

        public List<SelectListItem> Materias { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Grupos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PeriodosAcademicos { get; set; } = new List<SelectListItem>();
        
        [BindProperty(SupportsGet = true)]
        public int? MateriaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GradoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GrupoId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PeriodoId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Inicializar las listas, aunque ya tienen valores predeterminados
                Materias = new List<SelectListItem>();
                Grupos = new List<SelectListItem>();
                PeriodosAcademicos = new List<SelectListItem>();

                // Añade la opción predeterminada a cada lista
                Materias.Add(new SelectListItem("-- Seleccionar Materia --", ""));
                Grupos.Add(new SelectListItem("-- Seleccionar Curso --", ""));
                PeriodosAcademicos.Add(new SelectListItem("-- Seleccionar Periodo --", ""));

                // 1. Cargar lista de materias
                var materias = await _context.Materias.OrderBy(m => m.NomMateria).ToListAsync();
                if (materias != null && materias.Any())
                {
                    foreach (var materia in materias)
                    {
                        Materias.Add(new SelectListItem(
                            materia.NomMateria,
                            materia.Id.ToString(),
                            MateriaId.HasValue && MateriaId.Value == materia.Id));
                    }
                }

                // 2. Cargar la lista de grupos académicos
                var gruposAcademicos = await _grupoRepository.ObtenerGruposAsync();
                if (gruposAcademicos != null && gruposAcademicos.Any())
                {
                    foreach (var grupo in gruposAcademicos)
                    {
                        var nombreGrupo = $"{grupo.Grado.NomGrado} - Grupo {grupo.NomGrupo}";
                        Grupos.Add(new SelectListItem(
                            nombreGrupo,
                            grupo.Id.ToString(),
                            GrupoId.HasValue && GrupoId.Value == grupo.Id));
                    }
                }

                // 3. Cargar periodos académicos activos
                var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
                if (periodos != null && periodos.Any())
                {
                    // Obtenemos el ID del primer periodo activo
                    int periodoActivoId = periodos[0].Id;

                    // Obtenemos los periodos académicos asociados a ese periodo
                    var periodosAcademicos = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(periodoActivoId);

                    if (periodosAcademicos != null && periodosAcademicos.Any())
                    {
                        foreach (var periodo in periodosAcademicos)
                        {
                            var nombrePeriodo = $"Periodo {periodo.NumeroPeriodo} - {periodo.Nombre}";
                            PeriodosAcademicos.Add(new SelectListItem(
                                nombrePeriodo,
                                periodo.Id.ToString(),
                                PeriodoId.HasValue && PeriodoId.Value == periodo.Id));
                        }

                        // Si no hay un periodo seleccionado, usar el primero
                        if (!PeriodoId.HasValue && periodosAcademicos.Any())
                        {
                            PeriodoId = periodosAcademicos.First().Id;
                        }
                    }
                }

                // 4. Si hay materia, grupo y periodo seleccionados, cargar las notas
                if (MateriaId.HasValue && GrupoId.HasValue && PeriodoId.HasValue)
                {
                    // Obtener nombre de la materia seleccionada
                    var materiaSeleccionada = materias?.FirstOrDefault(m => m.Id == MateriaId.Value);
                    NombreMateria = materiaSeleccionada?.NomMateria ?? "Materia no encontrada";

                    // Obtener nombre del grupo seleccionado
                    var grupoSeleccionado = await _grupoRepository.ObtenerGrupoPorIdAsync(GrupoId.Value);
                    if (grupoSeleccionado != null)
                    {
                        NombreGrupo = $"{grupoSeleccionado.Grado.NomGrado} - Grupo {grupoSeleccionado.NomGrupo}";
                        Grados = NombreGrupo;
                    }
                    else
                    {
                        NombreGrupo = "Grupo no encontrado";
                        Grados = "Grupo no encontrado";
                    }

                    // Cargar notas por materia y grupo
                    await CargarNotasPorMateria(MateriaId.Value, GrupoId.Value, PeriodoId.Value);
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar los datos: {ex.Message}";
            }

            return Page();
        }

        private async Task CargarNotasPorMateria(int idMateria, int idGrupo, int idPeriodo)
        {
            // Inicializar la lista vacía
            EstudiantesNotas = new List<EstudianteNotasMateriaDTO>();
            PromedioGeneralCurso = 0;

            try
            {
                // 1. Obtener todas las matrículas asociadas al grupo académico
                var matriculas = await _context.Matriculas
                    .Where(m => m.IdGrupoAcad == idGrupo && m.Activo.ToLower() == "si")
                    .ToListAsync();

                if (matriculas == null || !matriculas.Any())
                {
                    MensajeError = "No se encontraron estudiantes matriculados en este curso.";
                    return;
                }

                // 2. Para cada matrícula, buscar el alumno y sus notas en la materia específica
                foreach (var matricula in matriculas)
                {
                    // Obtener datos del alumno
                    var alumno = await _context.Alumnos
                        .Include(a => a.Participante)
                        .FirstOrDefaultAsync(a => a.Codigo == matricula.Codigo);

                    if (alumno == null) continue;

                    // Buscar las notas del estudiante
                    var notasEstudiante = await _notasRepository.ObtenerNotasPorMatricula(matricula.IdMatricula);

                    // Filtrar por periodo académico y materia
                    var notasFiltradas = notasEstudiante?
                        .Where(n => n.IdPeriodoAcademico == idPeriodo && n.IdMateria == idMateria)
                        .ToList() ?? new List<NotaModels>();

                    // Crear el modelo para la vista
                    var estudianteViewModel = new EstudianteNotasMateriaDTO
                    {
                        IdMatricula = matricula.IdMatricula,
                        Codigo = matricula.Codigo,
                        NombreCompleto = $"{alumno.Participante.Nombre} {alumno.Participante.Apellido}",
                        Notas = new List<NotaDTO>(),
                        PromedioSaber = 0,
                        PromedioHacer = 0,
                        PromedioSer = 0,
                        PromedioFinal = 0
                    };

                    // Agregar las notas si existen
                    if (notasFiltradas.Any())
                    {
                        // Mapear todas las notas individuales sin filtrar
                        estudianteViewModel.Notas = notasFiltradas.Select(n => new NotaDTO
                        {
                            IdNota = n.IdNota,
                            NotaSaber = n.NotaSaber,
                            NotaHacer = n.NotaHacer,
                            NotaSer = n.NotaSer,
                            Observacion = n.Observacion
                        }).ToList();

                        // Calcular promedios
                        if (estudianteViewModel.Notas.Any())
                        {
                            estudianteViewModel.PromedioSaber = estudianteViewModel.Notas
                                .Where(n => n.NotaSaber.HasValue)
                                .Select(n => n.NotaSaber.Value)
                                .DefaultIfEmpty(0)
                                .Average();

                            estudianteViewModel.PromedioHacer = estudianteViewModel.Notas
                                .Where(n => n.NotaHacer.HasValue)
                                .Select(n => n.NotaHacer.Value)
                                .DefaultIfEmpty(0)
                                .Average();

                            estudianteViewModel.PromedioSer = estudianteViewModel.Notas
                                .Where(n => n.NotaSer.HasValue)
                                .Select(n => n.NotaSer.Value)
                                .DefaultIfEmpty(0)
                                .Average();

                            estudianteViewModel.PromedioFinal = CalcularNotaFinal(
                                estudianteViewModel.PromedioSaber,
                                estudianteViewModel.PromedioHacer,
                                estudianteViewModel.PromedioSer);
                        }
                    }

                    EstudiantesNotas.Add(estudianteViewModel);
                }

                // Ordenar por promedio final (descendente)
                if (EstudiantesNotas.Any())
                {
                    EstudiantesNotas = EstudiantesNotas.OrderByDescending(e => e.PromedioFinal).ToList();
                }

                // Calcular promedio general del curso en esta materia
                if (EstudiantesNotas.Any(e => e.Notas.Any()))
                {
                    PromedioGeneralCurso = EstudiantesNotas
                        .Where(e => e.Notas.Any())
                        .Select(e => e.PromedioFinal)
                        .DefaultIfEmpty(0)
                        .Average();
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar las notas: {ex.Message}";
            }
        }

        // Método para calcular la nota final con ponderaciones
        private decimal CalcularNotaFinal(decimal? notaSaber, decimal? notaHacer, decimal? notaSer)
        {
            decimal saber = notaSaber.GetValueOrDefault() * 0.3m;
            decimal hacer = notaHacer.GetValueOrDefault() * 0.4m;
            decimal ser = notaSer.GetValueOrDefault() * 0.3m;

            return saber + hacer + ser;
        }
    }
}