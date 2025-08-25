using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AcademicoSFA.Infrastructure;
using X.PagedList.Extensions;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using AcademicoSFA.Infrastructure.Repositories;

namespace AcademicoSFA.Pages.Materia
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly MateriaRepository _materiaRepository;

        public IndexModel(MateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public IPagedList<Domain.Entities.Materia> Materia { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public int? Pagina { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TerminoBusqueda { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            int pageNumber = Pagina ?? 1;

            var query1 = string.IsNullOrEmpty(TerminoBusqueda)
                ? await _materiaRepository.GetAllMateriasAsync()
                : await _materiaRepository.GetMateriasByNombreAsync(TerminoBusqueda);

            Materia = query1
                .OrderBy(m => m.NomMateria)
                .ToPagedList(pageNumber, PageSize);
        }
    }
}
