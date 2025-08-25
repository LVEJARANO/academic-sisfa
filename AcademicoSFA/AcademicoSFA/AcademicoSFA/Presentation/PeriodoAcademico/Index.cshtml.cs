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
        public int A�oLectivoActivo { get; set; }

        public async Task OnGetAsync()
        {
            // Obtener periodos activos usando el m�todo existente
            var periodosActivos = await _periodoRepository.GetAllPeriodosActivosAsync();

            // Si hay periodos activos, usar el primero como a�o lectivo activo
            if (periodosActivos.Any())
            {
                A�oLectivoActivo = periodosActivos.First().Id;

                // Filtrar los periodos acad�micos por el a�o lectivo activo
                PeriodoAcademico = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(A�oLectivoActivo);
            }
            else
            {
                // Si no hay a�o lectivo activo, mostrar todos los periodos
                PeriodoAcademico = await _periodoAcademicoRepository.GetAllPeriodosAcademicosAsync();
            }
        }
    }
}