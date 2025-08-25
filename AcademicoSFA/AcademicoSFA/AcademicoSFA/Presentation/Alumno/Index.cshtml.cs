using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.Alumno;
[Authorize]
public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IParticipanteRepository _participanteRepository;
    private readonly INotyfService _servicioNotificacion;

    // Constructor que inicializa el repositorio y la configuración
    public IndexModel(IAlumnoRepository alumnoRepository,IConfiguration configuration, IParticipanteRepository participanteRepository, INotyfService servicioNotificacion)
    {
        _configuration = configuration;
        _alumnoRepository = alumnoRepository;
        _participanteRepository = participanteRepository;
        _servicioNotificacion = servicioNotificacion;
    }
    public IList<ActiveStudentDTO> Alumno { get; set; } = default!;
    public IPagedList<ActiveStudentDTO> AlumnoPagedList { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Pagina { get; set; } 
    [BindProperty(SupportsGet = true)]
    public string TerminoBusqueda { get; set; }
    public int PageSize { get; set; } = 10;

    // Método que maneja las solicitudes GET
    public async Task OnGetAsync()
    {
        try
        {
            int pageNumber = Pagina ?? 1;
            Alumno = await _alumnoRepository.ObtenerAlumnosFiltradosAsync(TerminoBusqueda);
            AlumnoPagedList = Alumno.AsQueryable().ToPagedList(pageNumber, PageSize);
        }
        catch (NpgsqlException)
        {
            _servicioNotificacion.Error("No se pudo cargar la lista de administrativos.");
        }
    }
}
