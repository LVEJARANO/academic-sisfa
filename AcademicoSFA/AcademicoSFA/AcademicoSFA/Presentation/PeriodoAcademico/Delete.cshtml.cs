using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.PeriodoAcademico
{
    //[Authorize]
    public class DeleteModel : PageModel
    {
        private readonly PeriodoAcademicoRepository _periodoAcademicoRepository;

        public DeleteModel(PeriodoAcademicoRepository periodoAcademicoRepository)
        {
            _periodoAcademicoRepository = periodoAcademicoRepository;
        }

        [BindProperty]
        public Domain.Entities.PeriodoAcademico PeriodoAcademico { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var periodoAcademico = await _periodoAcademicoRepository.GetPeriodoAcademicoByIdAsync(id);

            if (periodoAcademico == null)
            {
                return NotFound();
            }
            else
            {
                PeriodoAcademico = periodoAcademico;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var periodoAcademico = await _periodoAcademicoRepository.GetPeriodoAcademicoByIdAsync(id);

            if (periodoAcademico != null)
            {
                await _periodoAcademicoRepository.DeletePeriodoAcademicoAsync(id);
            }

            return RedirectToPage("./Index");
        }
    }
}
