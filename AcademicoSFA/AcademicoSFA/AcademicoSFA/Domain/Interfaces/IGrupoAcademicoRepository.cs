using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IGrupoAcademicoRepository
    {
        Task<List<GrupoAcad>> ObtenerGruposAsync();
        Task<List<GrupoAcad>> ObtenerGruposPorPeriodoAsync(int idPeriodo);
        Task<GrupoAcad> ObtenerGrupoPorIdAsync(int idGrupo);
        Task<List<GrupoAcad>> ObtenerGruposPorGradoAsync(int idGrado);
        Task<List<GrupoAcadMateria>> ObtenerMateriasDeGrupoAsync(int idGrupoAcad);
    }
}
