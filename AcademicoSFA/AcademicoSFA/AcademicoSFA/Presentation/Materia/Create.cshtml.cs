using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.Materia
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly INotyfService _servicioNotificacion;
        private readonly MateriaRepository _materiaRepository;
        public CreateModel(MateriaRepository materiaRepository,INotyfService servicioNotificacion)
        {
            _materiaRepository = materiaRepository;
            _servicioNotificacion = servicioNotificacion;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Domain.Entities.Materia Materia { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario.");
                return Page();
            }
            // Verificar si la materia ya está registrada
            var materiaExistente = await _materiaRepository.GetMateriasByNombreAsync(Materia.NomMateria);
            if (materiaExistente.Count>0)
            {
                _servicioNotificacion.Warning("Esta materia ya está registrada en la base de datos.");
                return Page(); 
            }
            // Si no existe, se agrega la materia usando el procedimiento almacenado
            await _materiaRepository.InsertMateriaAsync(Materia.NomMateria);
            _servicioNotificacion.Success("La materia se registró exitosamente.");
            return RedirectToPage("./Index");
        }
    }
}
