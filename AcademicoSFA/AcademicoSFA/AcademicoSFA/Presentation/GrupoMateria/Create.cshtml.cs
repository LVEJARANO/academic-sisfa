using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;  // Servicio de notificación
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.GrupoAcadMateria
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;  // Inyectamos el servicio de notificación

        public CreateModel(SfaDbContext context, INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.GrupoAcadMateria GrupoAcadMateria { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Cargar la lista de grupos académicos concatenando Grado y Grupo
            var gruposAcad = await _context.GruposAcad
                .Include(g => g.Grado)
                .Select(g => new {
                    IdGrupoAcad = g.Id,
                    DisplayText = g.Grado.NomGrado + " - " + g.NomGrupo // Concatenar grado y grupo
                }).ToListAsync();

            // Cargar la lista de materias
            var materias = await _context.Materias
                .Select(m => new {
                    IdMateria = m.Id,
                    DisplayText = m.NomMateria
                }).ToListAsync();

            // Pasar los datos a las vistas como SelectLists
            ViewData["GruposAcad"] = new SelectList(gruposAcad, "IdGrupoAcad", "DisplayText");
            ViewData["Materias"] = new SelectList(materias, "IdMateria", "DisplayText");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Warning("Hay problemas en el formulario, revisa los campos.");
                return Page();
            }

            // Validar si ya existe la combinación de IdGrupoAcad y IdMateria
            var asignacionExistente = await _context.GrupoAcadMateria
                .AnyAsync(gm => gm.IdGrupoAcad == GrupoAcadMateria.IdGrupoAcad && gm.IdMateria == GrupoAcadMateria.IdMateria);

            if (asignacionExistente)
            {
                await OnGetAsync(); // Recargar los dropdowns
                _servicioNotificacion.Warning("Esta materia ya ha sido asignada a este grupo.");
                return Page();
            }

            try
            {
                _context.GrupoAcadMateria.Add(GrupoAcadMateria);
                await _context.SaveChangesAsync();
                // Mostrar mensaje de éxito
                _servicioNotificacion.Success("Asignación creada exitosamente.");
                return RedirectToPage("./Index");
            }
            catch (DbUpdateException)
            {
                // Manejar cualquier error al guardar en la base de datos
                _servicioNotificacion.Error("Error al intentar guardar la asignación.");
                return Page();
            }
        }
    }
}
