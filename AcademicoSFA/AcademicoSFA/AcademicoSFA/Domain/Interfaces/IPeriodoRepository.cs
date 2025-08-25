using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface IPeriodoRepository
    {
        // ==== Métodos para PeriodoAcademico ====
        /// Obtiene todos los registros de PeriodoAcademico, incluyendo la navegación a Periodo.
        Task<List<PeriodoAcademico>> GetAllPeriodosAcademicosAsync();
        /// Obtiene los PeriodoAcademico filtrados por IdPeriodo (año lectivo).
        Task<List<PeriodoAcademico>> GetPeriodosAcademicosByPeriodoIdAsync(int periodoId);
        /// Obtiene un PeriodoAcademico por su Id.
        Task<PeriodoAcademico> GetPeriodoAcademicoByIdAsync(int id);
        /// Crea un nuevo registro de PeriodoAcademico.
        Task CreatePeriodoAcademicoAsync(PeriodoAcademico periodoAcademico);
        /// Actualiza un PeriodoAcademico existente.
        Task<bool> UpdatePeriodoAcademicoAsync(PeriodoAcademico periodoAcademico);
        /// Elimina un PeriodoAcademico por su Id.
        Task DeletePeriodoAcademicoAsync(int id);
        /// Verifica si existe un PeriodoAcademico con el Id indicado.
        Task<bool> PeriodoAcademicoExistsAsync(int id);
        /// Retorna todos los Periodos (años lectivos) cuyo campo Activo == "SI".
        Task<List<Periodo>> GetActivePeriodosAsync();
        /// Obtiene el Id del PeriodoAcademico actual basado en la fecha de hoy.
        Task<int> ObtenerPeriodoAcademicoActualAsync();
        /// Versión síncrona de ObtenerPeriodoAcademicoActualAsync.
        int ObtenerPeriodoAcademicoActual();

        // ==== Métodos para Periodo ====

        /// Obtiene todos los registros de Periodo.
        Task<List<Periodo>> GetAllPeriodosAsync();
        /// Obtiene todos los registros de Periodo cuyo campo Activo == "SI".
        Task<List<Periodo>> GetAllPeriodosActivosAsync();
        /// Cambia el estado (Activo/Inactivo) de un Periodo.
        Task UpdateEstadoPeriodoAsync(int idPeriodo);
        /// Actualiza el año (AnioEscolar) de un Periodo.
        Task UpdateAnioPeriodoAsync(int idPeriodo, string anio);
    }
}
