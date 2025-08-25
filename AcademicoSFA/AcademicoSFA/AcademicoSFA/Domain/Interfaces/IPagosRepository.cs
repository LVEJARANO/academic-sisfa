using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IPagosRepository
    {
        Task<List<DetallePagoModels>> ConsultarPagoPorMatricula(int idMatricula);
        Task<bool> EliminarPagoAsync(int id);
    }
}
