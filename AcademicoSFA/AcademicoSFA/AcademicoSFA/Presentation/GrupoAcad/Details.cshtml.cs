using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.GrupoAcad
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly SfaDbContext _context;

        public DetailsModel(SfaDbContext context)
        {
            _context = context;
        }

        public Domain.Entities.GrupoAcad GrupoAcad { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoacad = await _context.GruposAcad.FirstOrDefaultAsync(m => m.Id == id);
            if (grupoacad == null)
            {
                return NotFound();
            }
            else
            {
                GrupoAcad = grupoacad;
            }
            return Page();
        }
    }
}
