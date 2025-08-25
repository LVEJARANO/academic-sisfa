using AcademicoSFA.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.GrupoAcad
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;

        public IndexModel(SfaDbContext context)
        {
            _context = context;
        }

        public IPagedList<Domain.Entities.GrupoAcad> GrupoAcad { get;set; } = default!;
        [BindProperty(SupportsGet = true)]
        public int? Pagina { get; set; }  // El número de página actual
        [BindProperty(SupportsGet = true)]
        public string TerminoBusqueda { get; set; }
        public int PageSize { get; set; } = 10;  // Cantidad de registros por página (puedes ajustar esto)

        public void OnGetAsync()
        {
            // Número de página, si es nulo se asigna a 1
            int pageNumber = Pagina ?? 1;

            // Crear una consulta base (usamos AsQueryable para poder aplicar filtros dinámicos)
            var query = _context.GruposAcad
                .Include(g => g.Grado)
                .Include(g => g.Periodo)
                .AsQueryable();

            // Si el término de búsqueda no está vacío, aplicar filtro
            if (!string.IsNullOrEmpty(TerminoBusqueda))
            {
                query = query.Where(g =>
                    g.NomGrupo.Contains(TerminoBusqueda) ||  
                    g.Grado.NomGrado.Contains(TerminoBusqueda));
            }

            GrupoAcad = query
                .OrderBy(g => g.Grado)  
                .ToPagedList(pageNumber, PageSize);
        }
    }
}
