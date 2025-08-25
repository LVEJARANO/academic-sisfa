using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicoSFA.Pages.PeriodoAcademico
{
    public class IndexModel : PageModel
    {
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly IPeriodoRepository _periodoRepository;

        public IndexModel(
            PeriodoAcademicoRepository periodoAcademicoRepository,
            IPeriodoRepository periodoRepository)
        {
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _periodoRepository = periodoRepository;
        }

        public IList<Domain.Entities.PeriodoAcademico> PeriodoAcademico { get; set; } = default!;
        public int AñoLectivoActivo { get; set; }

        public async Task OnGetAsync()
        {
            // Obtener periodos activos usando el método existente
            var periodosActivos = await _periodoRepository.GetAllPeriodosActivosAsync();

            // Si hay periodos activos, usar el primero como año lectivo activo
            if (periodosActivos.Any())
            {
                AñoLectivoActivo = periodosActivos.First().Id;

                // Filtrar los periodos académicos por el año lectivo activo
                PeriodoAcademico = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(AñoLectivoActivo);
            }
            else
            {
                // Si no hay año lectivo activo, mostrar todos los periodos
                PeriodoAcademico = await _periodoAcademicoRepository.GetAllPeriodosAcademicosAsync();
            }
        }
    }
}