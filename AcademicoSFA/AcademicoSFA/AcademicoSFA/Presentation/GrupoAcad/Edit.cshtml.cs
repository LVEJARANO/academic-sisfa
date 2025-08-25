using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoAcad
{
    [Authorize]
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
        public Domain.Entities.GrupoAcad GrupoAcad { get; set; } = default!;
        public async Task<IActionResult> OnGetAsync(int? id)
        {

            if (id == null)
            {
                _servicioNotificacion.Error("ID no proporcionado para el grupo académico.");
                return NotFound();
            }

            GrupoAcad = await _context.GruposAcad.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

            if (GrupoAcad == null)
            {
                _servicioNotificacion.Warning("El grupo académico no fue encontrado.");
                return NotFound();
            }


            // Poblar las listas desplegables con los valores correctos
            ViewData["IdGrado"] = new SelectList(await _context.Grados.ToListAsync(), "Id", "NomGrado", GrupoAcad.IdGrado);
            ViewData["IdPeriodo"] = new SelectList(await _context.Periodos.ToListAsync(), "Id", "AnioEscolar", GrupoAcad.IdPeriodo);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    _servicioNotificacion.Error("Hay errores en el formulario. Por favor corrige los problemas y vuelve a intentarlo.");
            //    // Volver a cargar los select lists si el formulario tiene errores
            //    ViewData["IdGrado"] = new SelectList(_context.Grados, "Id", "NomGrado", IdGrado);
            //    ViewData["IdPeriodo"] = new SelectList(_context.Set<AcademicoSFA.Models.Periodo>(), "Id", "AnioEscolar", IdPeriodo);
            //    return Page();
            //}

            //if (!ModelState.IsValid)
            //{
            //    foreach (var entry in ModelState)
            //    {
            //        foreach (var error in entry.Value.Errors)
            //        {
            //            Console.WriteLine($"Error en {entry.Key}: {error.ErrorMessage}");
            //        }
            //    }

            //    _servicioNotificacion.Error("Hay errores en el formulario. Por favor corrige los problemas y vuelve a intentarlo.");
            //    return Page();
            //}


            try
            {
                _context.Attach(GrupoAcad).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("El grupo académico fue editado exitosamente.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrupoAcadExists(GrupoAcad.Id))
                {
                    _servicioNotificacion.Warning("El grupo académico que intentas editar ya no existe.");
                    return NotFound();
                }
                else
                {
                    _servicioNotificacion.Error("Ocurrió un problema al actualizar el grupo académico. Inténtalo nuevamente.");
                    throw;
                }
            }

            return RedirectToPage("./Index");

        }

        private bool GrupoAcadExists(int id)
        {
            return _context.GruposAcad.Any(e => e.Id == id);
        }
    }
}
