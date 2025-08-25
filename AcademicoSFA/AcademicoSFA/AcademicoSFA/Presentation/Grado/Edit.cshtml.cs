using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.Grado
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        public EditModel(SfaDbContext context,INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Grado Grado { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grado =  await _context.Grados.FirstOrDefaultAsync(m => m.Id == id);
            if (grado == null)
            {
                return NotFound();
            }
            Grado = grado;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Grado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradoExists(Grado.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            _servicioNotificacion.Success("El grado se ha modificado satisfactoriamente.");
            return RedirectToPage("./Index");
        }

        private bool GradoExists(int id)
        {
            return _context.Grados.Any(e => e.Id == id);
        }
    }
}
