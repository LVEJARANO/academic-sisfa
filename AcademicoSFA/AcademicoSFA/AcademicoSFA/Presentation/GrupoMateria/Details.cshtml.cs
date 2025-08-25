using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.GrupoMateria
{
    //[Authorize]
    public class DetailsModel : PageModel
    {
        private readonly SfaDbContext _context;

        public DetailsModel(SfaDbContext context)
        {
            _context = context;
        }

        public Domain.Entities.GrupoAcadMateria GrupoAcadMateria { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grupoacadmateria = await _context.GrupoAcadMateria.FirstOrDefaultAsync(m => m.Id == id);
            if (grupoacadmateria == null)
            {
                return NotFound();
            }
            else
            {
                GrupoAcadMateria = grupoacadmateria;
            }
            return Page();
        }
    }
}
