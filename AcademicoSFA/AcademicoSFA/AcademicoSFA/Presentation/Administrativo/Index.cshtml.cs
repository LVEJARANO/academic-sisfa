using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using X.PagedList;
using X.PagedList.Extensions;

namespace AcademicoSFA.Pages.Administrativo
{
    public class IndexModel : PageModel
    {
        private readonly SfaDbContext _context;
        private readonly INotyfService _servicioNotificacion;
        private readonly IParticipanteRepository _participanteRepository;

        public IndexModel(SfaDbContext context, INotyfService servicioNotificacion, IParticipanteRepository participanteRepository)
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
                string rol = "ADMIN";
                var participantesLista = await _participanteRepository.ObtenerParticipantesFiltradosAsync(TerminoBusqueda,rol);
                Participantes = participantesLista;
                ParticipantesPagedList = Participantes.AsQueryable().ToPagedList(pageNumber, PageSize);
            }
            catch (NpgsqlException)
            {
                _servicioNotificacion.Error("No se pudo cargar la lista de administrativos.");
                Participantes = new List<Domain.Entities.Participante>();
                ParticipantesPagedList = new PagedList<Domain.Entities.Participante>(
                    new List<Domain.Entities.Participante>(), 1, 10);
            }
        }
    }
}