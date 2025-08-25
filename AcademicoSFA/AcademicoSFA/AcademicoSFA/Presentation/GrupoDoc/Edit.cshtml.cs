using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Pages.GrupoDoc
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;

        public EditModel(SfaDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public GrupoDocente GrupoDocente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GrupoDocente = await _context.GruposDoc.FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoDocente == null)
            {
                return NotFound();
            }

            // Cargar la lista de docentes
            var docentes = await _context.Participantes
                .Where(p => p.Rol == "DOCENTE") // Filtrar solo los docentes
                .Select(p => new {
                    IdParticipante = p.Id,
                    DisplayText = p.Nombre + " " + p.Apellido
                }).ToListAsync();

            // Cargar la lista de grupos académicos concatenados con el grado
            var gruposAcad = await _context.GruposAcad
                .Include(g => g.Grado)
                .Select(g => new {
                    IdGrupoAcad = g.Id,
                    DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo
                }).ToListAsync();

            // Pasar las listas a la vista a través de ViewData
            ViewData["Docentes"] = new SelectList(docentes, "IdParticipante", "DisplayText", GrupoDocente.IdParticipante);
            ViewData["GruposAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText", GrupoDocente.IdGrupoAcad);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Recargar los dropdowns si hay errores en el formulario
                var docentes = await _context.Participantes
                    .Where(p => p.Rol == "DOCENTE")
                    .Select(p => new
                    {
                        IdParticipante = p.Id,
                        DisplayText = p.Nombre + " " + p.Apellido
                    }).ToListAsync();

                var gruposAcad = await _context.GruposAcad
                    .Include(g => g.Grado)
                    .Select(g => new
                    {
                        IdGrupoAcad = g.Id,
                        DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo
                    }).ToListAsync();

                ViewData["Docentes"] = new SelectList(docentes, "IdParticipante", "DisplayText", GrupoDocente.IdParticipante);
                ViewData["GruposAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText", GrupoDocente.IdGrupoAcad);

                return Page();
            }

            _context.Attach(GrupoDocente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrupoDocenteExists(GrupoDocente.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool GrupoDocenteExists(int id)
        {
            return _context.GruposDoc.Any(e => e.Id == id);
        }
    }
}
