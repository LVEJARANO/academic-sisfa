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
using AcademicoSFA.Domain.Monitoring; // ? NUEVA LÍNEA
using System.Diagnostics; // ? NUEVA LÍNEA

namespace AcademicoSFA.Pages.InicioSesion
{
    public class LoginModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly IPasswordHasher<Domain.Entities.Participante> _passwordHasher;
        private readonly ISisfaMetrics _metrics; // ? NUEVA LÍNEA
        private readonly ILogger<LoginModel> _logger; // ? NUEVA LÍNEA

        public LoginModel(SfaDbContext context,
            IPasswordHasher<Domain.Entities.Participante> passwordHasher,
            ISisfaMetrics metrics, // ? NUEVA LÍNEA
            ILogger<LoginModel> logger) // ? NUEVA LÍNEA
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _metrics = metrics; // ? NUEVA LÍNEA
            _logger = logger; // ? NUEVA LÍNEA
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class LoginInputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Clave { get; set; }

            [Display(Name = "Recordarme")]
            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
            // Registrar vista de página
            _metrics.RecordPageView("Login"); // ? NUEVA LÍNEA
            ErrorMessage = string.Empty;
        }

        public async Task<IActionResult> OnPostLoginAsync()
        {
            var stopwatch = Stopwatch.StartNew(); // ? NUEVA LÍNEA

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                _logger.LogInformation("Iniciando intento de login para usuario: {Email}", Input.Email); // ? NUEVA LÍNEA

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
                        // ¡Autenticación exitosa!
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

                        // ===== REGISTRAR MÉTRICAS DE ÉXITO =====
                        _metrics.RecordLoginAttempt(true, Input.Email); // ? NUEVA LÍNEA
                        _logger.LogInformation("Login exitoso para usuario {Email} en {Duration}ms",
                            Input.Email, stopwatch.ElapsedMilliseconds); // ? NUEVA LÍNEA

                        return RedirectToPage("/Dashboard/Dashboard");
                    }
                    else
                    {
                        // ===== REGISTRAR MÉTRICAS DE FALLA =====
                        _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA LÍNEA
                        _logger.LogWarning("Intento de login fallido para usuario {Email} - contraseña incorrecta", Input.Email); // ? NUEVA LÍNEA

                        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                        return Page();
                    }
                }
                else
                {
                    // ===== REGISTRAR MÉTRICAS DE USUARIO NO ENCONTRADO =====
                    _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA LÍNEA
                    _logger.LogWarning("Intento de login fallido para usuario {Email} - usuario no encontrado", Input.Email); // ? NUEVA LÍNEA

                    ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // ===== REGISTRAR MÉTRICAS DE ERROR =====
                _metrics.RecordLoginAttempt(false, Input.Email); // ? NUEVA LÍNEA
                _logger.LogError(ex, "Error inesperado durante login para usuario {Email}", Input.Email); // ? NUEVA LÍNEA

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