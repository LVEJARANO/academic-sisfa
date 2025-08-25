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

namespace AcademicoSFA.Pages.GrupoDoc
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly SfaDbContext _context;

        public DetailsModel(SfaDbContext context)
        {
            _context = context;
        }

        public GrupoDocente GrupoDocente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupodocente = await _context.GruposDoc.FirstOrDefaultAsync(m => m.Id == id);
            if (grupodocente == null)
            {
                return NotFound();
            }
            else
            {
                GrupoDocente = grupodocente;
            }
            return Page();
        }
    }
}
