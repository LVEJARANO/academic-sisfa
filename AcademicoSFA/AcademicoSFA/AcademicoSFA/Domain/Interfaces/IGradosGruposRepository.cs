using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IGradosGruposRepository
    {
        Task<List<GradosGrupos>> ObtenerGradosGruposAsync();
    }
}
