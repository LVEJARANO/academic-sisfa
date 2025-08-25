using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IParticipanteRepository
    {
        // Inserta un nuevo participante y retorna el ID generado
        Task<int> InsertParticipante(string nombres, string apellidos, string documento, string email, string rol);

        // Retorna participantes según documento
        Task<List<Participante>> ObtenerParticipantePorDocumento(string documento);

        // Retorna participantes según ID
        Task<List<Participante>> ObtenerParticipantePorId(int idPart);

        // Actualiza los datos del participante
        Task<int> UpdateParticipante(int idPart, string nombres, string apellidos, string documento, string email, string rol);

        // Elimina lógicamente un participante
        Task UpdateDeleteParticipanteAsync(int idPart);

        // Retorna todos los docentes activos
        Task<List<Participante>> ObtenerDocentesAsync();

        // Verifica si ya existe un email registrado
        Task<bool> ExisteEmailAsync(string email);

        // Verifica si ya existe un código de matrícula (alumno)
        Task<bool> ExisteCodigoAsync(string codigo);

        // Retorna todos los administradores activos
        Task<List<Participante>> ObtenerAdminAsync();
        // Término de búsqueda
        Task<List<Participante>> ObtenerParticipantesFiltradosAsync(string? terminoBusqueda, string rol);

    }
}
