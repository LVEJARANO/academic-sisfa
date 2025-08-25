using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Pago
{
    //[Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        public DeleteModel(SfaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PagoModel PagoModel { get; set; } = default!;
        [BindProperty]
        public DetallePagoModels DetallePagoModels { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagomodel = await _context.Pagos.FirstOrDefaultAsync(m => m.Id == id);

            if (pagomodel == null)
            {
                return NotFound();
            }
            else
            {
                PagoModel = pagomodel;
            }


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pagomodel = await _context.Pagos.FindAsync(id);
            if (pagomodel != null)
            {
                PagoModel = pagomodel;
                _context.Pagos.Remove(PagoModel);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
