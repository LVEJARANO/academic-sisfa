using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions; // Incluir servicio de notificaciones
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Materia
{
    //[Authorize]
    public class EditModel : PageModel
    {
        private readonly MateriaRepository _materiaRepository;
        private readonly INotyfService _servicioNotificacion; // Servicio de notificaciones

        public EditModel(MateriaRepository materiaRepository,INotyfService servicioNotificacion)
        {
            _materiaRepository = materiaRepository;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.Materia Materia { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id == 0)
            {
                _servicioNotificacion.Error("El ID de la materia es inválido.");
                return NotFound();
            }
            var materia1 = await _materiaRepository.GetMateriaByIdAsync(id);
            if (materia1 == null)
            {
                _servicioNotificacion.Error("No se encontró la materia que deseas editar.");
                return NotFound();
            }

            Materia = materia1;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Warning("Corrige los errores en el formulario antes de continuar.");
                return Page();
            }
            try
            {

                await _materiaRepository.UpdateMateriaAsync(Materia.Id, Materia.NomMateria);
                _servicioNotificacion.Success("La materia se ha actualizado exitosamente.");
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción de la base de datos
                _servicioNotificacion.Error($"Ocurrió un error al intentar actualizar la materia: {ex.Message}");
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
