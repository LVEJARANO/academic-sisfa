using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Pages.Pago
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly SfaDbContext _context;

        public DetailsModel(SfaDbContext context)
        {
            _context = context;
        }

        public PagoModel PagoModel { get; set; } = default!;

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
    }
}
