using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Pages.DocenteMateria
{
    //[Authorize]
    public class DeleteModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly IMateriaRepository _materiasADocentesRepository;
        public DeleteModel(SfaDbContext context, INotyfService servicioNotificacion, IMateriaRepository materiasADocentesRepository)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _materiasADocentesRepository = materiasADocentesRepository;
        }

        [BindProperty]
        public Domain.Entities.DocenteMateriaGrupoAcad DocenteMateriaGrupoAcad { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Warning("No se ha proporcionado un ID para eliminar.");
                return NotFound();
            }
            DocenteMateriaGrupoAcad = await _materiasADocentesRepository.GetDocenteMateriaGrupoAcadDetalladoAsync(id.Value);


            if (DocenteMateriaGrupoAcad == null)
            {
                _servicioNotificacion.Warning("No se ha encontrado el registro para eliminar.");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                _servicioNotificacion.Warning("No se ha proporcionado un ID para eliminar.");
                return NotFound();
            }

            DocenteMateriaGrupoAcad = await _context.DocenteMateriasGrupoAcad.FindAsync(id);

            if (DocenteMateriaGrupoAcad != null)
            {
                _context.DocenteMateriasGrupoAcad.Remove(DocenteMateriaGrupoAcad);
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("La asignación de materia a docente ha sido eliminada exitosamente.");
            }
            else
            {
                _servicioNotificacion.Error("No se pudo eliminar el registro. Posiblemente ya fue eliminado.");
                return NotFound();
            }
            return RedirectToPage("./Index");
        }
    }
}
