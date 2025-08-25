using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Pago
{
    //[Authorize]
    public class EditModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;

        public EditModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public int MatriculaId { get; set; }

        [BindProperty]
        public bool PagoAlDia { get; set; }

        [BindProperty]
        public string? Observaciones { get; set; }

        public AlumnoModel Alumno { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int id)
        {
            var matricula = await _context.Matriculas
                .Include(m => m.Alumno)
                .ThenInclude(a => a.Participante)
                .FirstOrDefaultAsync(m => m.IdMatricula == id);

            if (matricula == null)
            {
                return NotFound();
            }

            MatriculaId = matricula.IdMatricula;
            PagoAlDia = matricula.PagoAlDia;
            Alumno = matricula.Alumno;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Warning("Por favor, corrija los errores en el formulario.");
                return Page();
            }

            var matricula = await _context.Matriculas.FindAsync(MatriculaId);

            if (matricula == null)
            {
                return NotFound();
            }

            matricula.PagoAlDia = PagoAlDia;

            try
            {
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("Estado de pago actualizado correctamente.");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error("Ocurrió un error al actualizar el estado de pago.");
                return Page();
            }
        }


    }
}
