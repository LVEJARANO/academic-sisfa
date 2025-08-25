using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoAcad
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;

        public DeleteModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.GrupoAcad GrupoAcad { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Warning("No se especificó un grupo académico válido para eliminar.");
                return NotFound();
            }

            // Cargar las relaciones de Grado y Periodo junto con GrupoAcad
            GrupoAcad = await _context.GruposAcad
                .Include(g => g.Periodo)  // Incluir la relación con Periodo
                .Include(g => g.Grado)    // Incluir la relación con Grado
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoAcad == null)
            {
                _servicioNotificacion.Warning("El grupo académico no se encontró.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Error("No se pudo eliminar el grupo académico.");
                return NotFound();
            }

            GrupoAcad = await _context.GruposAcad.FindAsync(id);

            if (GrupoAcad != null)
            {
                try
                {
                    _context.GruposAcad.Remove(GrupoAcad);
                    await _context.SaveChangesAsync();
                    _servicioNotificacion.Success("El grupo académico ha sido eliminado exitosamente.");
                }
                catch (DbUpdateException ex)
                {
                    // Manejar la excepción de llave foránea
                    _servicioNotificacion.Error("No se puede eliminar este grupo académico porque tiene registros asociados.");
                    return Page();  // Vuelve a la página de eliminación
                }
            }

            return RedirectToPage("./Index");
        }
    }
}
