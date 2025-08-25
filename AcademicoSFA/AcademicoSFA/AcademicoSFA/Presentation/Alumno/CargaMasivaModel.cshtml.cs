using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AcademicoSFA.Domain.Entities;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text.RegularExpressions;
using AcademicoSFA.Domain.Interfaces;

namespace AcademicoSFA.Pages.Alumno;
//[Authorize]
public class CargaMasivaModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly IParticipanteRepository _repParticipante;
    private readonly IAlumnoRepository _repAlumno;
    private readonly INotyfService _servicioNotificacion;

    public CargaMasivaModel(SfaDbContext context, IParticipanteRepository repParticipante, IAlumnoRepository repAlumno, INotyfService servicioNotificacion)
    {
        _context = context;
        _servicioNotificacion = servicioNotificacion;
        _repParticipante = repParticipante;
        _repAlumno = repAlumno;

        // Configuración para EPPlus
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    public class ResultadoCargaMasiva
    {
        public int RegistrosExitosos { get; set; } = 0;
        public int RegistrosExistentes { get; set; } = 0;
        public List<string> ErroresValidacion { get; set; } = new List<string>();
    }

    [BindProperty]
    public ResultadoCargaMasiva? ResultadoCarga { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile archivoExcel)
    {
        ResultadoCarga = new ResultadoCargaMasiva();

        if (archivoExcel == null || archivoExcel.Length == 0)
        {
            _servicioNotificacion.Error("Debe seleccionar un archivo Excel válido.");
            return Page();
        }

        if (!Path.GetExtension(archivoExcel.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !Path.GetExtension(archivoExcel.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            _servicioNotificacion.Error("El archivo debe ser de tipo Excel (.xlsx o .xls).");
            return Page();
        }

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await archivoExcel.CopyToAsync(memoryStream);
                using (var package = new ExcelPackage(memoryStream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Primera hoja de trabajo

                    int rowCount = worksheet.Dimension.Rows;
                    int successCount = 0;

                    // Comenzamos desde la fila 2 (la primera fila es el encabezado)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Verificar si la fila está vacía
                        if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 4].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text))
                        {
                            continue; // Saltamos filas vacías
                        }

                        var nombres = worksheet.Cells[row, 1].Text.Trim();
                        var apellidos = worksheet.Cells[row, 2].Text.Trim();
                        var documento = worksheet.Cells[row, 3].Text.Trim();
                        var email = worksheet.Cells[row, 4].Text.Trim();
                        var codigo = worksheet.Cells[row, 5].Text.Trim();

                        // Validaciones
                        bool esValido = true;

                        // Validación de nombres (solo letras y espacios)
                        if (string.IsNullOrWhiteSpace(nombres) || !Regex.IsMatch(nombres, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Nombres inválidos. Debe contener solo letras.");
                            esValido = false;
                        }

                        // Validación de apellidos (solo letras y espacios)
                        if (string.IsNullOrWhiteSpace(apellidos) || !Regex.IsMatch(apellidos, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Apellidos inválidos. Debe contener solo letras.");
                            esValido = false;
                        }

                        // Validación de documento (solo números)
                        if (string.IsNullOrWhiteSpace(documento) || !Regex.IsMatch(documento, @"^\d+$"))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Documento inválido. Debe contener solo números.");
                            esValido = false;
                        }

                        // Validación de email
                        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Email inválido.");
                            esValido = false;
                        }

                        // Validación de código (TAM)
                        if (string.IsNullOrWhiteSpace(codigo))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: TAM inválido.");
                            esValido = false;
                        }

                        // Verificar si el email ya existe
                        var emailExiste = await _repParticipante.ExisteEmailAsync(email);
                        if (emailExiste)
                        {
                            ResultadoCarga.RegistrosExistentes++;
                            continue;
                        }

                        // Verificar si el código ya existe
                        var codigoExiste = await _repParticipante.ExisteCodigoAsync(codigo);
                        if (codigoExiste)
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: El TAM {codigo} ya está registrado en el sistema.");
                            esValido = false;
                        }

                        if (!esValido)
                        {
                            continue; // Si hay errores, pasamos a la siguiente fila
                        }

                        // Procesar registro si es válido
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                // Verificar si el participante ya existe por documento
                                var idParticipante = await _repParticipante.ObtenerParticipantePorDocumento(documento);
                                int idPart;

                                if (idParticipante.Count > 0)
                                {
                                    idPart = idParticipante[0].Id;
                                }
                                else
                                {
                                    string rol = "ESTUDIANTE";
                                    idPart = await _repParticipante.InsertParticipante(nombres, apellidos, documento, email, rol);

                                    if (idPart <= 0)
                                    {
                                        ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Error al insertar el participante.");
                                        await transaction.RollbackAsync();
                                        continue;
                                    }
                                }

                                // Insertar alumno
                                string estado = "Activo";
                                int resultado = await _repAlumno.InsertAlumno(codigo, idPart, estado);

                                if (resultado == 1)
                                {
                                    await transaction.CommitAsync();
                                    successCount++;
                                }
                                else
                                {
                                    await transaction.RollbackAsync();
                                    ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Error al insertar el alumno.");
                                }
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Error de procesamiento: {ex.Message}");
                            }
                        }
                    }

                    ResultadoCarga.RegistrosExitosos = successCount;

                    if (successCount > 0)
                    {
                        _servicioNotificacion.Success($"Se han registrado exitosamente {successCount} estudiantes.");
                    }
                    else if (ResultadoCarga.ErroresValidacion.Count > 0)
                    {
                        _servicioNotificacion.Warning("No se pudo registrar ningún estudiante. Revise los errores de validación.");
                    }
                    else if (ResultadoCarga.RegistrosExistentes > 0)
                    {
                        _servicioNotificacion.Information($"Los {ResultadoCarga.RegistrosExistentes} registros ya existían en el sistema.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _servicioNotificacion.Error($"Error al procesar el archivo: {ex.Message}");
        }

        return Page();
    }

    public IActionResult OnGetDescargarPlantilla()
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Plantilla Alumnos");

        // Definir encabezados
        worksheet.Cells[1, 1].Value = "Nombres *";
        worksheet.Cells[1, 2].Value = "Apellidos *";
        worksheet.Cells[1, 3].Value = "Documento *";
        worksheet.Cells[1, 4].Value = "Email *";
        worksheet.Cells[1, 5].Value = "TAM *";

        // Estilo para los encabezados
        using (var range = worksheet.Cells[1, 1, 1, 5])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            range.Style.Font.Color.SetColor(Color.DarkBlue);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Agregar comentarios para ayudar al usuario
        var comment1 = worksheet.Cells[1, 1].AddComment("Ingrese solo letras y espacios. Ej: Juan Carlos", "Sistema Académico");
        comment1.AutoFit = true;

        var comment2 = worksheet.Cells[1, 2].AddComment("Ingrese solo letras y espacios. Ej: Pérez González", "Sistema Académico");
        comment2.AutoFit = true;

        var comment3 = worksheet.Cells[1, 3].AddComment("Ingrese solo números sin espacios ni caracteres especiales. Ej: 123456789", "Sistema Académico");
        comment3.AutoFit = true;

        var comment4 = worksheet.Cells[1, 4].AddComment("Ingrese un correo electrónico válido. Ej: usuario@dominio.com", "Sistema Académico");
        comment4.AutoFit = true;

        var comment5 = worksheet.Cells[1, 5].AddComment("Ingrese el código TAM único para el estudiante", "Sistema Académico");
        comment5.AutoFit = true;

        // Ajustar ancho de columnas
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 20;
        worksheet.Column(3).Width = 15;
        worksheet.Column(4).Width = 30;
        worksheet.Column(5).Width = 15;

        // Agregar fila de ejemplo
        worksheet.Cells[2, 1].Value = "Juan Carlos";
        worksheet.Cells[2, 2].Value = "Pérez González";
        worksheet.Cells[2, 3].Value = "123456789";
        worksheet.Cells[2, 4].Value = "juan.perez@ejemplo.com";
        worksheet.Cells[2, 5].Value = "12345";

        // Estilo de fila de ejemplo (color gris claro)
        using (var range = worksheet.Cells[2, 1, 2, 5])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.Font.Italic = true;
        }

        // Formato de alternancia de color para filas
        worksheet.Cells["A3:E50"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells["A3:E50"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));

        // Descargar el archivo
        var memoryStream = new MemoryStream();
        package.SaveAs(memoryStream);
        memoryStream.Position = 0;

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Plantilla_Carga_Alumnos.xlsx");
    }
}