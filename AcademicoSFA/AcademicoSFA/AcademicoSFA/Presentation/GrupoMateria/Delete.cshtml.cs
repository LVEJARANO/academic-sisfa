using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;  // Incluir servicio de notificaciones
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoMateria
{
   // [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;  // Servicio de notificaciones

        public DeleteModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.GrupoAcadMateria GrupoAcadMateria { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID del grupo académico-materia es inválido.");
                return NotFound();
            }

            GrupoAcadMateria = await _context.GrupoAcadMateria
                .Include(gam => gam.GrupoAcad)
                .ThenInclude(ga => ga.Grado)  // Incluir el Grado para la concatenación
                .Include(gam => gam.Materia)  // Incluir la materia
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoAcadMateria == null)
            {
                _servicioNotificacion.Error("No se encontró el registro que deseas eliminar.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("El ID del grupo académico-materia es inválido.");
                return NotFound();
            }

            GrupoAcadMateria = await _context.GrupoAcadMateria.FindAsync(id);

            if (GrupoAcadMateria != null)
            {
                try
                {
                    _context.GrupoAcadMateria.Remove(GrupoAcadMateria);
                    await _context.SaveChangesAsync();
                    _servicioNotificacion.Success("El registro se ha eliminado exitosamente.");
                }
                catch (DbUpdateException)
                {
                    _servicioNotificacion.Error("Error al eliminar el registro. Asegúrate de que no esté relacionado con otras entidades.");
                    return Page();
                }
            }
            else
            {
                _servicioNotificacion.Warning("El registro que intentas eliminar ya no existe.");
            }

            return RedirectToPage("./Index");
        }
    }
}
