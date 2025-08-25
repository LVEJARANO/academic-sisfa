using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IMatriculaRepository
    {
        Task<List<MatriculaInfo>> ObtenerMatriculasActivasAsync();
        Task<List<MatriculaPagosModels>> ObtenerMatriculasConPagosAsync();
        Task UpdateEstadoMatriculaAsync(string codigo);
        Task<int> ObtenerIdMatricula(string codigo);
        Task<string> ObtenerNumeroMatriculaParticipante(int idParticipante);

        Task<MatriculaModels> ObtenerMatriculaConGradoPorCodigoEstudiante(string codigo);
    }
}
