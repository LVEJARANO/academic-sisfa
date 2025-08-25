using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.Grado
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        public CreateModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Domain.Entities.Grado Grado { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Error("Error en el formulario");
                return Page();
            }

            _context.Grados.Add(Grado);
            await _context.SaveChangesAsync();
            _servicioNotificacion.Success("El grado se creo correctamente.");
            return RedirectToPage("./Index");
        }
    }
}
