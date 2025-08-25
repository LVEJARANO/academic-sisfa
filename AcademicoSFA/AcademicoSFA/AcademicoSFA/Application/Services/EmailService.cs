using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AcademicoSFA.Application.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarCorreoRecuperacionContrasena(string destinatario, string contrasenaTemp)
        {
            try
            {
                // Obtener configuración de correo del appsettings.json
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromPassword = _configuration["EmailSettings:FromPassword"];
                var fromName = _configuration["EmailSettings:FromName"];

                // Crear mensaje de correo
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Recuperación de Contraseña - Sistema Académico SFA",
                    Body = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
                                .header {{ background-color: #4361ee; padding: 15px; color: white; text-align: center; border-radius: 5px 5px 0 0; }}
                                .content {{ padding: 20px; }}
                                .password {{ font-size: 18px; font-weight: bold; background-color: #f5f5f5; padding: 10px; border-radius: 4px; text-align: center; margin: 15px 0; }}
                                .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #777; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h2>Sistema Académico SFA</h2>
                                </div>
                                <div class='content'>
                                    <p>Estimado/a usuario,</p>
                                    <p>Recibimos una solicitud para restablecer tu contraseña. Hemos generado una contraseña temporal para ti:</p>
                                    <div class='password'>{contrasenaTemp}</div>
                                    <p>Por favor, utiliza esta contraseña para iniciar sesión y luego cámbiala inmediatamente por una nueva de tu elección desde la opción 'Cambiar contraseña' en tu perfil.</p>
                                    <p>Si no solicitaste esta recuperación de contraseña, por favor contáctanos inmediatamente.</p>
                                    <p>Saludos cordiales,<br/>Equipo de Sistema Académico SFA</p>
                                </div>
                                <div class='footer'>
                                    Este es un correo electrónico automático, por favor no respondas a este mensaje.
                                </div>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(destinatario);

                // Configurar cliente SMTP
                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.EnableSsl = enableSsl;
                    smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword);

                    await smtpClient.SendMailAsync(mailMessage);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}