using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Pages.GrupoDoc
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _notyf;

        public DeleteModel(SfaDbContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        [BindProperty]
        public GrupoDocente GrupoDocente { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _notyf.Error("ID no proporcionado para la eliminación.");
                return NotFound();
            }

            GrupoDocente = await _context.GruposDoc
                .Include(g => g.Participante)
                .Include(g => g.GrupoAcad)
                .ThenInclude(ga => ga.Grado)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoDocente == null)
            {
                _notyf.Warning("El grupo docente no fue encontrado.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _notyf.Error("ID no proporcionado para la eliminación.");
                return NotFound();
            }

            GrupoDocente = await _context.GruposDoc.FindAsync(id);

            if (GrupoDocente == null)
            {
                _notyf.Warning("El grupo docente no existe o ya fue eliminado.");
                return NotFound();
            }

            _context.GruposDoc.Remove(GrupoDocente);
            await _context.SaveChangesAsync();
            _notyf.Success("El grupo docente fue eliminado exitosamente.");

            return RedirectToPage("./Index");
        }
    }
}
