using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IMateriaRepository
    {
        Task<List<Materia>> GetAllMateriasAsync();
        Task<List<Materia>> GetMateriasByNombreAsync(string nombreMateria);
        Task<Materia?> GetMateriaByIdAsync(int idMateria);
        Task InsertMateriaAsync(string nombreMateria);
        Task UpdateMateriaAsync(int id, string nombreMateria);
        Task<int> ObtenerIdMateria(string nombreMateria);

        // Métodos de materias asignadas a docentes
        Task<List<MateriasADocentesModels>> GetMateriasAsignadasADocentes();
        Task<DocenteMateriaGrupoAcad> GetDocenteMateriaGrupoAcadDetalladoAsync(int id);
    }
}
