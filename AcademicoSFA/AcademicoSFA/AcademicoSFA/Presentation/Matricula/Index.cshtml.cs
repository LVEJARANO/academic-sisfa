using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.Matricula
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly MatriculaRepository _matriculaService;

        public IndexModel(MatriculaRepository matriculaService)
        {
            _matriculaService = matriculaService;
        }

        public List<MatriculaInfo> Matriculas { get; set; }
        public IPagedList<MatriculaInfo> MatriculasPagedList { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Pagina { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TerminoBusqueda { get; set; }

        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            int pageNumber = Pagina ?? 1;

            // Obtener todas las matrículas
            Matriculas = await _matriculaService.ObtenerMatriculasActivasAsync();

            // Aplicar filtro si existe término de búsqueda
            if (!string.IsNullOrEmpty(TerminoBusqueda))
            {
                Matriculas = Matriculas
                    .Where(m =>
                        (m.Nombre != null && m.Nombre.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Codigo != null && m.Codigo.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Grado != null && m.Grado.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Email != null && m.Email.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            // Crear lista paginada
            MatriculasPagedList = Matriculas.AsQueryable().ToPagedList(pageNumber, PageSize);
        }
    }
}