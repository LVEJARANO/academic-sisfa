using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.Pago
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly MatriculaRepository _matriculaRepository;

        public IndexModel(SfaDbContext context, MatriculaRepository matriculaRepository)
        {
            _context = context;
            _matriculaRepository = matriculaRepository;
        }

        public IList<AlumnoModel> Alumnos { get; set; } = default!;
        public IList<MatriculaPagosModels> matriculaInfos { get; set; }
        public IPagedList<MatriculaPagosModels> MatriculasPagosPagedList { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Pagina { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TerminoBusqueda { get; set; }

        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            int pageNumber = Pagina ?? 1;

            // Obtener todas las matrículas con pagos
            matriculaInfos = await _matriculaRepository.ObtenerMatriculasConPagosAsync();

            // Aplicar filtro si existe término de búsqueda
            if (!string.IsNullOrEmpty(TerminoBusqueda))
            {
                matriculaInfos = matriculaInfos
                    .Where(m =>
                        (m.Nombre != null && m.Nombre.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Codigo != null && m.Codigo.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Documento != null && m.Documento.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Email != null && m.Email.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();
            }

            // Crear lista paginada
            MatriculasPagosPagedList = matriculaInfos.AsQueryable().ToPagedList(pageNumber, PageSize);
        }

        public async Task<IActionResult> OnPostUpdatePagoStatusAsync(int id, bool pagoAlDia)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
            {
                return NotFound();
            }
            matricula.PagoAlDia = pagoAlDia;
            await _context.SaveChangesAsync();
            //_servicioNotificacion.Success("Estado de pago actualizado correctamente.");
            return RedirectToPage("./Index");
        }
    }
}