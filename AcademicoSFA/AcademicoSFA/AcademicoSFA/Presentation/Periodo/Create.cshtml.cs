using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using AspNetCoreHero.ToastNotification.Abstractions;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Periodo
{
    //[Authorize]
    public class CreateModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IPeriodoRepository _periodoRepository;
        private readonly INotyfService _servicioNotificacion;

        public CreateModel(SfaDbContext context, IPeriodoRepository periodoRepository, INotyfService servicioNotificacion)
        {
            _context = context;
            _periodoRepository = periodoRepository;
            _servicioNotificacion = servicioNotificacion;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Domain.Entities.Periodo Periodo { get; set; } = default!;
        public IList<Domain.Entities.Periodo> PeriodoList { get; set; } = default!;
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            PeriodoList = await _periodoRepository.GetAllPeriodosActivosAsync();
            if (PeriodoList.Count>0)
            {
                _servicioNotificacion.Warning("No es posible crear un nuevo año lectivo mientras exista uno activo. Por favor, desactive el año lectivo activo antes de proceder.");
                return Page();
            }
            _context.Periodos.Add(Periodo);
            await _context.SaveChangesAsync();
            _servicioNotificacion.Success("Año lectivo creado exitosamente.");
            return RedirectToPage("./Index");
        }
    }
}
