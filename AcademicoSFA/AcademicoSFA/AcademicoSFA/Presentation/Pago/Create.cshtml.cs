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
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;

        public CreateModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public PagoModel PagoModel { get; set; } = default!;
        public AlumnoModel Alumno { get; set; } = default!;
        [BindProperty(SupportsGet = true)] 
        public int MatriculaId { get; set; }


        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Verificar que la matrícula exista
            var matricula = await _context.Matriculas
                .Include(m => m.Alumno)
                .ThenInclude(a => a.Participante)
                .FirstOrDefaultAsync(m => m.IdMatricula == id);

            if (matricula == null)
            {
                return NotFound();
            }

            MatriculaId = id;
            Alumno = matricula.Alumno;

            // Lista de meses para el dropdown
            ViewData["Meses"] = GetMesesDropdown();
            ViewData["TipoPagoList"] = GetTipoPagoDropdown();
            ViewData["EstadoPagoList"] = GetEstadoPagoDropdown();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Verificar que MatriculaId tenga un valor válido
            if (MatriculaId <= 0)
            {
                _servicioNotificacion.Warning("El ID de la matrícula no es válido.");
                return Page();
            }

            // Asignar el MatriculaId al PagoModel
            PagoModel.MatriculaId = MatriculaId;


            // Convertir FechaPago a UTC si no lo está
            if (PagoModel.FechaPago.Kind == DateTimeKind.Unspecified)
            {
                PagoModel.FechaPago = DateTime.SpecifyKind(PagoModel.FechaPago, DateTimeKind.Utc);
            }
            else if (PagoModel.FechaPago.Kind == DateTimeKind.Local)
            {
                PagoModel.FechaPago = PagoModel.FechaPago.ToUniversalTime();
            }
            try
            {
                // Agregar el PagoModel al contexto y guardar los cambios
                _context.Pagos.Add(PagoModel);
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("El pago se ha registrado correctamente.");
                // Redirigir a la página de índice después de guardar
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error("Ocurrió un error al registrar el pago. Por favor, inténtelo de nuevo.");
                // Recargar los datos necesarios para los dropdowns
                ViewData["Meses"] = GetMesesDropdown();
                ViewData["TipoPagoList"] = GetTipoPagoDropdown();
                ViewData["EstadoPagoList"] = GetEstadoPagoDropdown();

                return Page();
            }
        }

        // Método para obtener los meses del año en un dropdown
        private IEnumerable<SelectListItem> GetMesesDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Enero", Text = "Enero" },
                new SelectListItem { Value = "Febrero", Text = "Febrero" },
                new SelectListItem { Value = "Marzo", Text = "Marzo" },
                new SelectListItem { Value = "Abril", Text = "Abril" },
                new SelectListItem { Value = "Mayo", Text = "Mayo" },
                new SelectListItem { Value = "Junio", Text = "Junio" },
                new SelectListItem { Value = "Julio", Text = "Julio" },
                new SelectListItem { Value = "Agosto", Text = "Agosto" },
                new SelectListItem { Value = "Septiembre", Text = "Septiembre" },
                new SelectListItem { Value = "Octubre", Text = "Octubre" },
                new SelectListItem { Value = "Noviembre", Text = "Noviembre" },
                new SelectListItem { Value = "Diciembre", Text = "Diciembre" }
            };
        }

        // Método para obtener los tipos de pago
        private IEnumerable<SelectListItem> GetTipoPagoDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Mensualidad", Text = "Mensualidad" }
            };
        }

        // Método para obtener los estados de pago
        private IEnumerable<SelectListItem> GetEstadoPagoDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Pagado", Text = "Pagado" }
            };
        }
    }
}
