using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Grado
{
 //   [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IGradosGruposRepository _gradosGruposRepository;

        public IndexModel(SfaDbContext context, IGradosGruposRepository gradosGruposRepository)
        {
            _context = context;
            _gradosGruposRepository = gradosGruposRepository;
        }

        public IList<Domain.Entities.Grado> Grado { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Grado = await _context.Grados.ToListAsync();
        }
    }
}
