using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoDoc
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;

        public IndexModel(SfaDbContext context)
        {
            _context = context;
        }

        public IList<GrupoDocente> GrupoDocente { get;set; } = default!;

        public async Task OnGetAsync()
        {
            //GrupoDocente = await _context.GruposDoc.ToListAsync();
            GrupoDocente = await _context.GruposDoc
    .Include(gd => gd.Participante)  // Incluir la relación con el participante (docente)
    .Include(gd => gd.GrupoAcad)     // Incluir la relación con el grupo académico
    .ThenInclude(ga => ga.Grado)     // Incluir la relación con el grado
    .ToListAsync();
        }
    }
}
