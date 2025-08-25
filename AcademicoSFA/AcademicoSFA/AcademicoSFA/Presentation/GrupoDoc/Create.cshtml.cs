using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoDoc
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;

        public CreateModel(SfaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
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
            ViewData["Docentes"] = new SelectList(docentes, "IdParticipante", "DisplayText");
            ViewData["GruposAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText");

            return Page();
        }

        [BindProperty]
        public GrupoDocente GrupoDocente { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
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

                ViewData["Docentes"] = new SelectList(docentes, "IdParticipante", "DisplayText");
                ViewData["GruposAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText");
                return Page();
            }

            _context.GruposDoc.Add(GrupoDocente);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
