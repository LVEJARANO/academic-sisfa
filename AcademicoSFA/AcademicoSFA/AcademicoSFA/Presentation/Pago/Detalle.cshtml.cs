using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.Pago
{
    public class DetalleModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly PagosRepository _pagosRepository;
        public DetalleModel(SfaDbContext context, PagosRepository pagosRepository)
        {
            _pagosRepository = pagosRepository;
            _context = context;
        }

        [BindProperty]
        public IList<DetallePagoModels> DetallePagoModels { get; set; }
        public async Task OnGetAsync(int idMatricula)
        {
            DetallePagoModels = await _pagosRepository.ConsultarPagoPorMatricula(idMatricula);
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _pagosRepository.EliminarPagoAsync(id);

            if (resultado)
            {
                // Redirigir a la página de detalle después de eliminar
                return RedirectToPage("./Detalle", new { idMatricula = DetallePagoModels.FirstOrDefault()?.id_matricula });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No se pudo eliminar el pago.");
                return Page();
            }
        }

    }
}
