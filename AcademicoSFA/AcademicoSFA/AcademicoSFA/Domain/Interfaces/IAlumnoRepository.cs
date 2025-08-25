using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IAlumnoRepository
    {
        Task<List<ActiveStudentDTO>> GetActiveStudents();
        Task<int> InsertAlumno(string codigo, int idParticipante, string estado);
        Task UpdateEstadoAlumnoAsync(string codigo);
        // Término de búsqueda
        Task<List<ActiveStudentDTO>> ObtenerAlumnosFiltradosAsync(string? terminoBusqueda);
        Task<AlumnoModel?> ObtenerAlumnoConParticipantePorCodigoAsync(string codigo);
        Task<bool> GuardarCambiosAsync();
    }
}
