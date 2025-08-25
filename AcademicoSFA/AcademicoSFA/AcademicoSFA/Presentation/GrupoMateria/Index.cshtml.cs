using AcademicoSFA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoAcadMateria
{
   // [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;

        public IndexModel(SfaDbContext context)
        {
            _context = context;
        }

        public IList<Domain.Entities.GrupoAcadMateria> GrupoAcadMaterias { get; set; } = default!;

        public async Task OnGetAsync()
        {
            var gruposAcadMaterias = await _context.GrupoAcadMateria
                .Include(gam => gam.GrupoAcad)       // Incluimos la relación con GrupoAcad
                .ThenInclude(ga => ga.Grado)         // Incluimos la relación con Grado
                .Include(gam => gam.Materia)         // Incluimos la relación con Materia
                .Select(gam => new
                {
                    Id = gam.Id,
                    Materia = gam.Materia.NomMateria,
                    GrupoGrado = gam.GrupoAcad.Grado.NomGrado + " - " + gam.GrupoAcad.NomGrupo // Concatenamos Grado y Grupo
                })
                .ToListAsync();

            ViewData["GruposAcadMaterias"] = gruposAcadMaterias;
        }
    }
}
