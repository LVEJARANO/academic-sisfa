using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Text;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Application.Services;

namespace AcademicoSFA.Pages.InicioSesion
{
    public class OlvideContrasenaModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly IPasswordHasher<Domain.Entities.Participante> _passwordHasher;
        private readonly EmailService _emailService;

        public OlvideContrasenaModel(
            SfaDbContext context,
            INotyfService servicioNotificacion,
            IPasswordHasher<Domain.Entities.Participante> passwordHasher,
            EmailService emailService)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
        }

        [BindProperty]
        [Required(ErrorMessage = "El correo electr�nico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electr�nico no es v�lido")]
        public string Email { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet()
        {
            // Inicializar la p�gina
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Buscar el usuario por email
                var participante = await _context.Participantes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Email == Email);

                if (participante == null)
                {
                    // No revelamos si el correo existe o no por seguridad
                    SuccessMessage = "Si tu correo est� registrado, recibir�s instrucciones para recuperar tu contrase�a.";
                    return Page();
                }

                // Generar contrase�a temporal aleatoria
                string nuevaContrasena = GenerarContrasenaAleatoria(8);

                // Hashear la nueva contrase�a
                var hashedPassword = _passwordHasher.HashPassword(participante, nuevaContrasena);

                // Actualizar directamente en la base de datos usando los nombres correctos de tabla y columnas
                int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE tbl_participante SET clave = {0} WHERE id_participante = {1}",
                    hashedPassword, participante.Id);

                // Verificar que realmente se actualiz� en la base de datos
                if (rowsAffected == 0)
                {
                    ErrorMessage = "No se pudo actualizar la contrase�a en la base de datos.";
                    return Page();
                }

                // Enviar correo electr�nico con la contrase�a temporal
                bool emailEnviado = await _emailService.EnviarCorreoRecuperacionContrasena(
                    participante.Email, nuevaContrasena);

                if (emailEnviado)
                {
                    SuccessMessage = $"Se ha enviado un correo con instrucciones para recuperar tu contrase�a. Por favor revisa tu bandeja de entrada. Contrase�a temporal: {nuevaContrasena}";
                }
                else
                {
                    SuccessMessage = $"No se pudo enviar el correo. Tu nueva contrase�a temporal es: {nuevaContrasena}";
                }
            }
            catch (Exception ex)
            {
                // Capturar y mostrar cualquier error que ocurra
                ErrorMessage = $"Error al procesar la solicitud: {ex.Message}";
            }

            return Page();
        }

        private string GenerarContrasenaAleatoria(int longitud)
        {
            const string caracteresPermitidos = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder builder = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < longitud; i++)
            {
                int index = random.Next(0, caracteresPermitidos.Length);
                builder.Append(caracteresPermitidos[index]);
            }

            return builder.ToString();
        }
    }
}