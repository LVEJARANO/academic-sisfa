namespace AcademicoSFA.Application.DTOs
{
    public class MateriaNotasDetalladasDTO
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; }
        public List<NotaDetalladaDTO> Notas { get; set; } = new List<NotaDetalladaDTO>();
    }
}
