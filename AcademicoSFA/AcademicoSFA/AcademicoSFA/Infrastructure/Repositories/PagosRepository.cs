using AcademicoSFA.Domain.Entities;
using AcademicoSFA.Domain.Interfaces;
using AcademicoSFA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AcademicoSFA.Infrastructure.Repositories
{
    public class PagosRepository: IPagosRepository
    {
        private readonly SfaDbContext _context;

        public PagosRepository(SfaDbContext context)
        {
            _context = context;
        }
        public async Task<List<DetallePagoModels>> ConsultarPagoPorMatricula(int idMatricula)
        {
            return await _context.DetallePagoModels
                .FromSqlRaw("CALL ConsultarPagoPorMatricula({0})",idMatricula)
                .ToListAsync();
        }


        // Método para eliminar un pago por su ID
        public async Task<bool> EliminarPagoAsync(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago != null)
            {
                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
