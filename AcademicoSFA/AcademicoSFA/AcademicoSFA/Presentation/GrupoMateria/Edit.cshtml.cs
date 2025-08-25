using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;  // Servicio de notificación
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoMateria
{
   // [Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;  // Inyectar servicio de notificaciones

        public EditModel(SfaDbContext context, INotyfService servicioNotificacion)
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
                _servicioNotificacion.Error("El ID del grupo académico no es válido.");
                return NotFound();
            }

            GrupoAcadMateria = await _context.GrupoAcadMateria
                .Include(gam => gam.GrupoAcad)
                .ThenInclude(ga => ga.Grado)
                .Include(gam => gam.Materia)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoAcadMateria == null)
            {
                _servicioNotificacion.Error("No se encontró el registro solicitado.");
                return NotFound();
            }

            // Concatenar el grado y grupo para el dropdown
            var gruposAcad = await _context.GruposAcad
                .Include(g => g.Grado)
                .Select(g => new
                {
                    IdGrupoAcad = g.Id,
                    DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo
                })
                .ToListAsync();

            ViewData["IdGrupoAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText", GrupoAcadMateria.IdGrupoAcad);
            ViewData["IdMateria"] = new SelectList(_context.Materias, "Id", "NomMateria", GrupoAcadMateria.IdMateria);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Warning("Hay problemas en el formulario. Por favor, revisa los campos.");

                // Recargar los dropdowns si el modelo no es válido
                var gruposAcad = await _context.GruposAcad
                    .Include(g => g.Grado)
                    .Select(g => new
                    {
                        IdGrupoAcad = g.Id,
                        DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo
                    })
                    .ToListAsync();

                ViewData["IdGrupoAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText", GrupoAcadMateria.IdGrupoAcad);
                ViewData["IdMateria"] = new SelectList(_context.Materias, "Id", "NomMateria", GrupoAcadMateria.IdMateria);

                return Page();
            }

            _context.Attach(GrupoAcadMateria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("El grupo académico se ha actualizado exitosamente.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrupoAcadMateriaExists(GrupoAcadMateria.Id))
                {
                    _servicioNotificacion.Error("No se encontró el grupo académico para actualizar.");
                    return NotFound();
                }
                else
                {
                    _servicioNotificacion.Error("Ocurrió un error al intentar actualizar el grupo académico.");
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool GrupoAcadMateriaExists(int id)
        {
            return _context.GrupoAcadMateria.Any(e => e.Id == id);
        }
    }
}
