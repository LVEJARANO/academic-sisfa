using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcademicoSFA.Data;
using AcademicoSFA.Models;
using Microsoft.AspNetCore.Authorization;

namespace AcademicoSFA.Pages.Periodo
{
    //[Authorize]
    public class DetailsModel : PageModel
    {
        private readonly AcademicoSFA.Data.SfaDbContext _context;

        public DetailsModel(AcademicoSFA.Data.SfaDbContext context)
        {
            _context = context;
        }

        public AcademicoSFA.Models.Periodo Periodo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var periodo = await _context.Periodos.FirstOrDefaultAsync(m => m.Id == id);
            if (periodo == null)
            {
                return NotFound();
            }
            else
            {
                Periodo = periodo;
            }
            return Page();
        }
    }
}
