using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;

namespace AcademicoSFA.Domain.Interfaces
{
    public interface INotasRepository
    {
        Task<int> GuardarNotaAsync(
           int idMatricula,
           int idMateria,
           int idPeriodoAcademico,
           decimal? notaSaber,
           decimal? notaHacer,
           decimal? notaSer,
           string observacion);

        Task<List<NotaModels>> ObtenerNotasPorMatricula(int idMatricula);

        Task GuardarObservacion(
            int idMatricula,
            int idMateria,
            int idPeriodoAcademico,
            string observacion);

        Task<List<NotaModels>> ObtenerNotasEstudianteAsync(int idMatricula, int idMateria, int idPeriodoAcademico);

        Task<List<MateriaConNotasDTO>> ObtenerNotasAgrupadas(int idMatricula, int idPeriodo);
    }

}
