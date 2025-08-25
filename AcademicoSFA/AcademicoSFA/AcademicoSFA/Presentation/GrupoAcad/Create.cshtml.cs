using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;

namespace AcademicoSFA.Pages.GrupoAcad
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        public CreateModel(SfaDbContext context,INotyfService servicioNotificacion)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
        }

        [BindProperty]
        public Domain.Entities.GrupoAcad GrupoAcad { get; set; } = default!;

        public IList<Domain.Entities.Grado> Grados { get; set; } = default!;
        public IList<Domain.Entities.Periodo> Periodos { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Cargar las listas de Grados y Periodos
            Grados = await _context.Grados.ToListAsync();
            Periodos = await _context.Periodos.ToListAsync();

            if (Grados == null || Periodos == null)
            {
                _servicioNotificacion.Error("No se pudieron cargar los datos necesarios.");
                return Page(); // O redirigir a una página de error
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    _servicioNotificacion.Error("Es necesario corregir los problemas en el formulario.");

            //    return Page();
            //}

            //_context.GruposAcad.Add(GrupoAcad);
            //await _context.SaveChangesAsync();
            //_servicioNotificacion.Success("Grado creado exitosamente.");

            //return RedirectToPage("./Index");
            //if (!ModelState.IsValid)
            //{
            //    // Recargar los dropdowns si hay errores
            //    await OnGetAsync(); // Esto recarga los dropdowns si ocurre un error de validación
            //    _servicioNotificacion.Warning("Es necesario corregir los problemas en el formulario.");
            //    return Page();
            //}

            // Validación de unicidad: Verificar si ya existe un grupo con el mismo grado y periodo
            bool grupoExistente = await _context.GruposAcad
                .AnyAsync(g => g.IdGrado == GrupoAcad.IdGrado && g.IdPeriodo == GrupoAcad.IdPeriodo && g.NomGrupo == GrupoAcad.NomGrupo);

            if (grupoExistente)
            {
                await OnGetAsync(); // Recargar los dropdowns
                _servicioNotificacion.Warning("Este grupo académico ya existe para el grado y periodo seleccionados.");
                return Page();
            }

            // Si no existe, agregar el nuevo grupo
            try
            {
                _context.GruposAcad.Add(GrupoAcad);
                await _context.SaveChangesAsync();
                _servicioNotificacion.Success("El grupo académico ha sido creado exitosamente.");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _servicioNotificacion.Error($"Ocurrió un error al crear el grupo académico: {ex.Message}");
                return Page();
            }
        }
    }
}
