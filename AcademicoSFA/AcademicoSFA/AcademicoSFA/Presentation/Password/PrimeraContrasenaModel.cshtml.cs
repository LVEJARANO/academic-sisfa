using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Text;
using AcademicoSFA.Application.Services;
using System.Linq;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;

namespace AcademicoSFA.Pages.InicioSesion
{
    public class PrimeraContrasenaModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService? _servicioNotificacion;
        private readonly IPasswordHasher<Domain.Entities.Participante> _passwordHasher;
        private readonly ParticipanteRepository _participanteRepository;

        public PrimeraContrasenaModel(
            SfaDbContext context,
            INotyfService servicioNotificacion,
            IPasswordHasher<Domain.Entities.Participante> passwordHasher,
            ParticipanteRepository participanteRepository)
        {
            _context = context;
            _servicioNotificacion = servicioNotificacion;
            _passwordHasher = passwordHasher;
            _participanteRepository = participanteRepository;
        }

        [BindProperty]
        [Required(ErrorMessage = "El correo electr�nico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electr�nico no es v�lido")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "El documento de identidad es obligatorio")]
        public string Documento { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La contrase�a es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres de longitud.", MinimumLength = 5)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La confirmaci�n de contrase�a es obligatoria")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contrase�a y la confirmaci�n de contrase�a no coinciden.")]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public int IdParticipante { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool IdentidadVerificada { get; set; } = false;

        public void OnGet()
        {
            // Inicializar la p�gina
            IdentidadVerificada = false;
            ErrorMessage = null;
            SuccessMessage = null;
        }

        public async Task<IActionResult> OnPostVerificarIdentidadAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Documento))
            {
                ErrorMessage = "El correo electr�nico y el documento de identidad son obligatorios.";
                return Page();
            }

            try
            {
                // Buscar al participante por documento
                var participantes = await _participanteRepository.ObtenerParticipantePorDocumento(Documento);
                var participante = participantes.FirstOrDefault(p => p.Email.Equals(Email, StringComparison.OrdinalIgnoreCase));

                if (participante == null)
                {
                    ErrorMessage = "No se encontr� ning�n participante con el correo electr�nico y documento proporcionados.";
                    return Page();
                }

                // Verificar si ya tiene contrase�a asignada
                if (!string.IsNullOrEmpty(participante.Clave))
                {
                    ErrorMessage = "Ya tienes una contrase�a asignada. Si la has olvidado, utiliza la opci�n 'Olvid� mi contrase�a'.";
                    return Page();
                }

                // Guardar el ID del participante para el siguiente paso
                IdParticipante = participante.Id;
                IdentidadVerificada = true;
                // Importante: El ModelState por defecto mantiene los valores previos, debemos limpiarlo
                ModelState.Clear();
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al verificar la identidad: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEstablecerContrasenaAsync()
        {
            // Mostrar los errores del ModelState para depuraci�n
            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    // Log o mostrar el error
                    Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                }
            }

            // Validar si las contrase�as est�n vac�as
            if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ErrorMessage = "Debes ingresar y confirmar la contrase�a.";
                IdentidadVerificada = true;
                return Page();
            }

            // Validar si las contrase�as coinciden
            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contrase�as no coinciden.";
                IdentidadVerificada = true;
                return Page();
            }

            // Validar el ID del participante
            if (IdParticipante <= 0)
            {
                ErrorMessage = "No se ha identificado correctamente al participante. Por favor, vuelve a verificar tu identidad.";
                IdentidadVerificada = false;
                return Page();
            }

            try
            {
                // Buscar al participante por ID usando FindAsync
                var participante = await _context.Participantes.FindAsync(IdParticipante);

                if (participante == null)
                {
                    ErrorMessage = "No se encontr� el participante.";
                    IdentidadVerificada = false;
                    return Page();
                }

                // Hashear la contrase�a antes de guardarla (exactamente como en AsignarContrasenaModel)
                participante.Clave = _passwordHasher.HashPassword(participante, Password);

                // Actualizar usando Entity Framework (exactamente como en AsignarContrasenaModel)
                _context.Participantes.Update(participante);
                await _context.SaveChangesAsync();

                // Mensaje de �xito
                SuccessMessage = "Tu contrase�a ha sido establecida correctamente. Ahora puedes iniciar sesi�n.";
                if (_servicioNotificacion != null)
                {
                    _servicioNotificacion.Success("La contrase�a se ha asignado correctamente.");
                }

                // Reiniciar el estado de verificaci�n
                IdentidadVerificada = false;
                return Page();
            }
            catch (Exception ex)
            {
                // Capturar y mostrar cualquier error que ocurra
                ErrorMessage = $"Error al establecer la contrase�a: {ex.Message}";
                IdentidadVerificada = true;
                return Page();
            }
        }
    }
}