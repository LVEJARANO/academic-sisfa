using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcademicoSFA.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Periodo
{
    //[Authorize]
    //OK
    public class IndexModel : PageModel
    {
        private readonly IPeriodoRepository _periodoRepository;
        public IndexModel(IPeriodoRepository periodoRepository)
        {
            _periodoRepository = periodoRepository;
        }
        public IList<Domain.Entities.Periodo> Periodo { get;set; } = default!;
        public async Task OnGetAsync()
        {
            Periodo = await _periodoRepository.GetAllPeriodosAsync();
        }
    }
}
