using AcademicoSFA.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AcademicoSFA.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {

        private readonly ILogger<DashboardModel> _logger;
        private readonly MatriculaRepository _matriculaRepository; // Interfaz para el repositorio de matrículas

        public string NumeroMatricula { get; set; }

        public DashboardModel(ILogger<DashboardModel> logger, MatriculaRepository matriculaRepository)
        {
            _logger = logger;
            _matriculaRepository = matriculaRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var idUsuario = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Invitado";

            if (string.IsNullOrEmpty(idUsuario))
            {
                return RedirectToPage("/InicioSesion/Login");
            }

            // Si el usuario es estudiante, obtener su número de matrícula
            if (userRole == "Estudiante")
            {
                NumeroMatricula = await _matriculaRepository.ObtenerNumeroMatriculaParticipante(int.Parse(idUsuario));
            }

            return Page();
        }
    }
}
