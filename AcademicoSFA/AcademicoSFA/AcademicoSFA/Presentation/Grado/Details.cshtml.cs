using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.Grado
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly SfaDbContext _context;

        public DetailsModel(SfaDbContext context)
        {
            _context = context;
        }

        public Domain.Entities.Grado Grado { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grado = await _context.Grados.FirstOrDefaultAsync(m => m.Id == id);
            if (grado == null)
            {
                return NotFound();
            }
            else
            {
                Grado = grado;
            }
            return Page();
        }
    }
}
