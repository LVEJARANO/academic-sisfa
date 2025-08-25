using AcademicoSFA.Infrastructure.Data;
using AcademicoSFA.Infrastructure.Repositories;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.Participante;

//[Authorize]
public class IndexModel : PageModel
{
    private readonly SfaDbContext _context;
    private readonly INotyfService _servicioNotificacion;
    private readonly ParticipanteRepository _participanteRepository;

    public IndexModel(SfaDbContext context, INotyfService servicioNotificacion, ParticipanteRepository participanteRepository)
    {
        _context = context;
        _servicioNotificacion = servicioNotificacion;
        _participanteRepository = participanteRepository;
    }

    public IReadOnlyList<Domain.Entities.Participante> Participantes { get; set; } = default!;
    public IPagedList<Domain.Entities.Participante> ParticipantesPagedList { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Pagina { get; set; }

    [BindProperty(SupportsGet = true)]
    public string TerminoBusqueda { get; set; }

    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        try
        {
            int pageNumber = Pagina ?? 1;

            // Obtener todos los docentes
            var participantesLista = await _participanteRepository.ObtenerDocentesAsync();
            Participantes = participantesLista;

            // Aplicar filtro si existe término de búsqueda
            if (!string.IsNullOrEmpty(TerminoBusqueda))
            {
                var filtrado = participantesLista
                    .Where(p =>
                        (p.Nombre != null && p.Nombre.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Apellido != null && p.Apellido.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Documento != null && p.Documento.Contains(TerminoBusqueda, StringComparison.OrdinalIgnoreCase))
                    )
                    .ToList();

                Participantes = filtrado;
            }

            // Crear lista paginada
            ParticipantesPagedList = Participantes.AsQueryable().ToPagedList(pageNumber, PageSize);
        }
        catch (NpgsqlException)
        {
            _servicioNotificacion.Error("No se pudo cargar la lista de docentes.");
            Participantes = new List<Domain.Entities.Participante>();
            ParticipantesPagedList = new PagedList<Domain.Entities.Participante>(
                new List<Domain.Entities.Participante>(), 1, 10);
        }
    }
}