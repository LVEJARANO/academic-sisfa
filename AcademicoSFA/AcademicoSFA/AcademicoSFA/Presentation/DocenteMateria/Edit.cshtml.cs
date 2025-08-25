using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.DocenteMateria
{
    //[Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;

        public EditModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.DocenteMateriaGrupoAcad DocenteMateriaGrupoAcad { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Warning("ID no proporcionado para la asignación de materia a docente.");
                return NotFound();
            }

            // Obtener el registro de DocenteMateriaGrupoAcad
            DocenteMateriaGrupoAcad = await _context.DocenteMateriasGrupoAcad.FirstOrDefaultAsync(m => m.Id == id);

            if (DocenteMateriaGrupoAcad == null)
            {
                _servicioNotificacion.Warning("No se encontró la asignación de materia a docente.");
                return NotFound();
            }

            // Cargar lista de docentes con el nombre completo
            var docentes = await _context.Participantes
                .Where(p => p.Rol == "DOCENTE")
                .Select(p => new {
                    Id = p.Id,
                    NombreCompleto = p.Nombre + " " + p.Apellido
                })
                .ToListAsync();

            // Cargar lista de grupos concatenados con el grado
            var gruposAcad = await _context.GruposAcad
                .Include(g => g.Grado)
                .Select(g => new {
                    Id = g.Id,
                    DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo
                })
                .ToListAsync();

            // Cargar lista de materias
            var materias = await _context.Materias
                .Select(m => new {
                    Id = m.Id,
                    NomMateria = m.NomMateria
                })
                .ToListAsync();

            // Pasar los datos a la vista a través de ViewData
            ViewData["IdParticipante"] = new SelectList(docentes, "Id", "NombreCompleto", DocenteMateriaGrupoAcad.IdParticipante);
            ViewData["IdGrupoAcad"] = new SelectList(gruposAcad, "Id", "DisplayText", DocenteMateriaGrupoAcad.IdGrupoAcad);
            ViewData["IdMateria"] = new SelectList(materias, "Id", "NomMateria", DocenteMateriaGrupoAcad.IdMateria);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Error("Hay errores en el formulario. Por favor, corrija los problemas e inténtelo de nuevo.");
                await OnGetAsync(DocenteMateriaGrupoAcad.Id);  // Recargar datos
                return Page();
            }

            _context.Attach(DocenteMateriaGrupoAcad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("Asignación de materia a docente actualizada exitosamente.");

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocenteMateriaGrupoAcadExists(DocenteMateriaGrupoAcad.Id))
                {
                    _servicioNotificacion.Warning("No se encontró la asignación que está intentando actualizar.");

                    return NotFound();
                }
                else
                {
                    _servicioNotificacion.Error("Ocurrió un error al intentar actualizar la asignación. Inténtelo de nuevo.");

                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool DocenteMateriaGrupoAcadExists(int id)
        {
            return _context.DocenteMateriasGrupoAcad.Any(e => e.Id == id);
        }
    }
}
