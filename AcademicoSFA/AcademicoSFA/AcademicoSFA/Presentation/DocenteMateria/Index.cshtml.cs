using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcademicoSFA.Pages.DocenteMateria
{
    //[Authorize]
    public class IndexModel : PageModel
    {
        private readonly IMateriaRepository _materiasADocenteModels;
        public IndexModel(IMateriaRepository materiasADocentesModels)
        {
            _materiasADocenteModels = materiasADocentesModels;
        }
        public List<MateriasADocentesModels> MateriasADocente {get;set;}
        public async Task OnGetAsync()
        {
            MateriasADocente = await _materiasADocenteModels.GetMateriasAsignadasADocentes();
        }
    }
}
