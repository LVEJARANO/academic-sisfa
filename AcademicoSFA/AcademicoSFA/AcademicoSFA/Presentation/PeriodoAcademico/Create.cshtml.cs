using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcademicoSFA.Pages.PeriodoAcademico
{
    public class CreateModel : PageModel
    {
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;
        private readonly IPeriodoRepository _periodoRepository;

        public CreateModel(PeriodoAcademicoRepository periodoAcademicoRepository, IPeriodoRepository periodoRepository)
        {
            _periodoAcademicoRepository = periodoAcademicoRepository;
            _periodoRepository = periodoRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Solo permitimos seleccionar periodos activos
            var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
            ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar");
            return Page();
        }

        [BindProperty]
        public Domain.Entities.PeriodoAcademico PeriodoAcademico { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
            //    ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar");
            //    return Page();
            //}

            // Verificar que la fecha fin sea mayor a la fecha inicio
            if (PeriodoAcademico.FechaFin <= PeriodoAcademico.FechaInicio)
            {
                ModelState.AddModelError("PeriodoAcademico.FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
                var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
                ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar");
                return Page();
            }

            // Verificar que no exista otro periodo con el mismo número para el mismo año lectivo
            var periodosAcademicos = await _periodoAcademicoRepository.GetPeriodosAcademicosByPeriodoIdAsync(PeriodoAcademico.IdPeriodo);
            if (periodosAcademicos.Any(p => p.NumeroPeriodo == PeriodoAcademico.NumeroPeriodo))
            {
                ModelState.AddModelError("PeriodoAcademico.NumeroPeriodo", "Ya existe un periodo con este número para el año lectivo seleccionado");
                var periodos = await _periodoAcademicoRepository.GetActivePeriodosAsync();
                ViewData["IdPeriodo"] = new SelectList(periodos, "Id", "AnioEscolar");
                return Page();
            }

            await _periodoAcademicoRepository.CreatePeriodoAcademicoAsync(PeriodoAcademico);
            return RedirectToPage("./Index");
        }
    }
}
