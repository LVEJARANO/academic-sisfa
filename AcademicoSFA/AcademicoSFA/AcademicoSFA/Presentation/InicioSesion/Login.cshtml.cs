using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using AcademicoSFA.Application.Services;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Domain.Monitoring; // ? NUEVA L�NEA
using System.Diagnostics; // ? NUEVA L�NEA

namespace AcademicoSFA.Pages.InicioSesion
{
    public class LoginModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IPasswordHasher<Domain.Entities.Participante> _passwordHasher;
        private readonly ISisfaMetrics _metrics; // ? NUEVA L�NEA
        private readonly ILogger<LoginModel> _logger; // ? NUEVA L�NEA

        public LoginModel(SfaDbContext context,
            IPasswordHasher<Domain.Entities.Participante> passwordHasher,
            ISisfaMetrics metrics, // ? NUEVA L�NEA
            ILogger<LoginModel> logger) // ? NUEVA L�NEA
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _metrics = metrics; // ? NUEVA L�NEA
            _logger = logger; // ? NUEVA L�NEA
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "El correo electr�nico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Debe ingresar un correo electr�nico v�lido.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contrase�a es obligatoria.")]
            [DataType(DataType.Password)]
            public string Clave { get; set; }

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
            // Registrar vista de p�gina
            _metrics.RecordPageView("Login"); // ? NUEVA L�NEA
            ErrorMessage = string.Empty;
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var stopwatch = Stopwatch.StartNew(); // ? NUEVA L�NEA

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _logger.LogInformation("Iniciando intento de login para usuario: {Email}", Input.Email); // ? NUEVA L�NEA

                var usuario = await _context.Participantes
                    .FirstOrDefaultAsync(u => u.Email == Input.Email);

                if (usuario != null)
                {
                    var resultado = _passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.Clave,
                        Input.Clave
                    );

                    if (resultado == PasswordVerificationResult.Success)
                    {
                        // �Autenticaci�n exitosa!
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.Nombre),
                            new Claim(ClaimTypes.Email, usuario.Email),
                            new Claim(ClaimTypes.Role, usuario.Rol),
                            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString())
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = Input.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        // ===== REGISTRAR M�TRICAS DE �XITO =====
                        _metrics.RecordLoginAttempt(true, Input.Email); // ? NUEVA L�NEA
                        _logger.LogInformation("Login exitoso para usuario {Email} en {Duration}ms",
                            Input.Email, stopwatch.ElapsedMilliseconds); // ? NUEVA L�NEA

                        return RedirectToPage("/Dashboard/Dashboard");
                    }
                    else
                    {
                        // ===== REGISTRAR M�TRICAS DE FALLA =====
                        _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA L�NEA
                        _logger.LogWarning("Intento de login fallido para usuario {Email} - contrase�a incorrecta", Input.Email); // ? NUEVA L�NEA

                        ModelState.AddModelError(string.Empty, "Correo o contrase�a incorrectos.");
                        return Page();
                    }
                }
                else
                {
                    // ===== REGISTRAR M�TRICAS DE USUARIO NO ENCONTRADO =====
                    _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA L�NEA
                    _logger.LogWarning("Intento de login fallido para usuario {Email} - usuario no encontrado", Input.Email); // ? NUEVA L�NEA

                    ModelState.AddModelError(string.Empty, "Correo o contrase�a incorrectos.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // ===== REGISTRAR M�TRICAS DE ERROR =====
                _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA L�NEA
                _logger.LogError(ex, "Error inesperado durante login para usuario {Email}", Input.Email); // ? NUEVA L�NEA

                ModelState.AddModelError(string.Empty, "Error interno del servidor");
                return Page();
            }
        }

        public IActionResult OnPostGoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("/InicioSesion/GoogleCallback")
            };

            return Challenge(properties, "Google");
        }
    }
}