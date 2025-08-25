using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AcademicoSFA.Pages.Grado
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        public DeleteModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Grado Grado { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var grado = await _context.Grados.FirstOrDefaultAsync(m => m.Id == id);

            if (grado == null)
            {
                return NotFound();
            }
            else
            {
                Grado = grado;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            //if (id == null)
            //{
            //    _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario.");

            //    return NotFound();
            //}

            //var grado = await _context.Grados.FindAsync(id);
            //if (grado != null)
            //{
            //    Grado = grado;
            //    _context.Grados.Remove(Grado);
            //    await _context.SaveChangesAsync();
            //}
            //_servicioNotificacion.Success("El grado se ha eliminado satisfactoriamente.");
            //return RedirectToPage("./Index");
            if (id == null)
            {
                _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario.");
                return NotFound();
            }

            var grado = await _context.Grados.FindAsync(id);

            if (grado == null)
            {
                _servicioNotificacion.Error("No se encontró el grado a eliminar.");
                return NotFound();
            }

            try
            {
                _context.Grados.Remove(grado);
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("El grado se ha eliminado satisfactoriamente.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
            {
                // Manejar la excepción de clave foránea
                _servicioNotificacion.Error("No se puede eliminar este grado porque está asociado con otros grupos académicos.");
            }
            catch (Exception ex)
            {
                // Manejar cualquier otra excepción
                _servicioNotificacion.Error("Ocurrió un error inesperado: " + ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}
