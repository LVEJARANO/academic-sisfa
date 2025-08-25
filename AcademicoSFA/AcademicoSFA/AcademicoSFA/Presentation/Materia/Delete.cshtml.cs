using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AspNetCoreHero.ToastNotification.Abstractions;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.Materia
{

    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion; // Servicio de notificaciones

        public DeleteModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Materia Materia { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID de la materia es inválido.");
                return NotFound();
            }

            var materia = await _context.Materias.FirstOrDefaultAsync(m => m.Id == id);

            if (materia == null)
            {
                _servicioNotificacion.Error("No se encontró la materia que deseas eliminar.");
                return NotFound();
            }
            else
            {
                Materia = materia;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID de la materia es inválido.");
                return NotFound();
            }

            var materia = await _context.Materias.FindAsync(id);
            if (materia != null)
            {
                try
                {
                    Materia = materia;
                    _context.Materias.Remove(Materia);
                    await _context.SaveChangesAsync();
                    _servicioNotificacion.Success("La materia ha sido eliminada exitosamente.");
                }
                catch (DbUpdateException)
                {
                    _servicioNotificacion.Error("No se puede eliminar la materia porque está relacionada con otros registros.");
                    return RedirectToPage("./Index");
                }
            }
            else
            {
                _servicioNotificacion.Warning("No se encontró la materia que deseas eliminar.");
            }

            return RedirectToPage("./Index");
        }
    }
}
