using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace AcademicoSFA.Pages.Matricula;
//[Authorize]
public class CargaMasivaMatriculasModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly GradosGruposRepository _grupoService;
    private readonly INotyfService _servicioNotificacion;

    public CargaMasivaMatriculasModel(SfaDbContext context, GradosGruposRepository grupoService, INotyfService servicioNotificacion)
    {
        _context = context;
        _grupoService = grupoService;
        _servicioNotificacion = servicioNotificacion;

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

    public List<GradosGrupos> GradosGrupos { get; set; } = new List<GradosGrupos>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Cargar los datos de grados y grupos
        GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile archivoExcel)
    {
        ResultadoCarga = new ResultadoCargaMasiva();

        // Obtener el periodo activo
        var periodoActivo = await _context.Periodos.FirstOrDefaultAsync(p => p.Activo == "SI");
        if (periodoActivo == null)
        {
            _servicioNotificacion.Error("No hay un periodo activo actualmente. No se pueden registrar matrículas.");
            ResultadoCarga.ErroresValidacion.Add("No hay un periodo activo actualmente. No se pueden registrar matrículas.");
            return Page();
        }

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

        // Cargar la lista de grados y grupos para validación
        GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();

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
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text))
                        {
                            continue; // Saltamos filas vacías
                        }

                        var tam = worksheet.Cells[row, 1].Text.Trim();
                        var grupoNombre = worksheet.Cells[row, 2].Text.Trim();

                        // Validaciones
                        bool esValido = true;

                        // Validación de TAM
                        if (string.IsNullOrWhiteSpace(tam))
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: TAM no puede estar vacío.");
                            esValido = false;
                        }

                        // Verificar si el alumno existe
                        var alumnoExiste = await _context.Alumnos.AnyAsync(a => a.Codigo == tam);
                        if (!alumnoExiste)
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: El TAM {tam} no corresponde a ningún alumno registrado.");
                            esValido = false;
                        }

                        // Verificar si el grupo académico existe
                        var grupoAcad = GradosGrupos.FirstOrDefault(g => g.GradoGrupo == grupoNombre);
                        if (grupoAcad == null)
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: El grupo '{grupoNombre}' no existe en el sistema.");
                            esValido = false;
                        }

                        // Verificar si la matrícula ya existe para este alumno en el periodo activo
                        var matriculaExiste = await _context.Matriculas
                            .AnyAsync(m => m.Codigo == tam && m.IdPeriodo == periodoActivo.Id && m.Activo == "SI");

                        if (matriculaExiste)
                        {
                            ResultadoCarga.RegistrosExistentes++;
                            continue;
                        }

                        if (!esValido)
                        {
                            continue; // Si hay errores, pasamos a la siguiente fila
                        }

                        // Procesar registro si es válido
                        try
                        {
                            var nuevaMatricula = new MatriculaModels
                            {
                                IdMatricula = 0,  // Se asignará automáticamente por la base de datos
                                Codigo = tam,
                                IdPeriodo = periodoActivo.Id,
                                Activo = "SI",
                                IdGrupoAcad = grupoAcad.IdGrupoAcad,
                                PagoAlDia = false // Por defecto, el pago no está al día
                            };

                            _context.Matriculas.Add(nuevaMatricula);
                            await _context.SaveChangesAsync();
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            ResultadoCarga.ErroresValidacion.Add($"Fila {row}: Error al insertar la matrícula: {ex.Message}");
                        }
                    }

                    ResultadoCarga.RegistrosExitosos = successCount;

                    if (successCount > 0)
                    {
                        _servicioNotificacion.Success($"Se han registrado exitosamente {successCount} matrículas.");
                    }
                    else if (ResultadoCarga.ErroresValidacion.Count > 0)
                    {
                        _servicioNotificacion.Warning("No se pudo registrar ninguna matrícula. Revise los errores de validación.");
                    }
                    else if (ResultadoCarga.RegistrosExistentes > 0)
                    {
                        _servicioNotificacion.Information($"Las {ResultadoCarga.RegistrosExistentes} matrículas ya existían en el sistema.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _servicioNotificacion.Error($"Error al procesar el archivo: {ex.Message}");
        }

        // Recargar los datos de grados y grupos
        GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();

        return Page();
    }

    public async Task<IActionResult> OnGetDescargarPlantillaAsync()
    {
        // Cargar la lista de grados y grupos para incluirlos en la plantilla
        GradosGrupos = await _grupoService.ObtenerGradosGruposAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Plantilla Matrículas");

        // Definir encabezados
        worksheet.Cells[1, 1].Value = "TAM *";
        worksheet.Cells[1, 2].Value = "Grupo Académico *";

        // Estilo para los encabezados
        using (var range = worksheet.Cells[1, 1, 1, 2])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
            range.Style.Font.Color.SetColor(Color.DarkBlue);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // Agregar comentarios para ayudar al usuario
        var comment1 = worksheet.Cells[1, 1].AddComment("Ingrese el código TAM del estudiante. Debe existir previamente en el sistema.", "Sistema Académico");
        comment1.AutoFit = true;

        var comment2 = worksheet.Cells[1, 2].AddComment("Seleccione el grupo académico de la lista desplegable.", "Sistema Académico");
        comment2.AutoFit = true;

        // Ajustar ancho de columnas
        worksheet.Column(1).Width = 20;
        worksheet.Column(2).Width = 30;

        // Configurar validación de datos para la columna de grupos académicos (lista desplegable)
        var groupValidation = worksheet.DataValidations.AddListValidation("B2:B1000");
        groupValidation.ShowErrorMessage = true;
        groupValidation.ErrorTitle = "Grupo Inválido";
        groupValidation.Error = "Por favor seleccione un grupo de la lista";

        // Agregar los grupos académicos a la validación
        foreach (var grupo in GradosGrupos)
        {
            groupValidation.Formula.Values.Add(grupo.GradoGrupo);
        }

        // Agregar fila de ejemplo
        if (GradosGrupos.Count > 0)
        {
            worksheet.Cells[2, 1].Value = "TAM12345";
            worksheet.Cells[2, 2].Value = GradosGrupos.First().GradoGrupo;
        }
        else
        {
            worksheet.Cells[2, 1].Value = "TAM12345";
            worksheet.Cells[2, 2].Value = "1-A";
        }

        // Estilo de fila de ejemplo (color gris claro)
        using (var range = worksheet.Cells[2, 1, 2, 2])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            range.Style.Font.Italic = true;
        }

        // Formato de alternancia de color para filas
        worksheet.Cells["A3:B100"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells["A3:B100"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));

        // Descargar el archivo
        var memoryStream = new MemoryStream();
        package.SaveAs(memoryStream);
        memoryStream.Position = 0;

        return File(
            memoryStream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Plantilla_Carga_Matriculas.xlsx");
    }
}