namespace AcademicoSFA.Application.DTOs
{
    public class MateriaConNotasDTO
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; }
        public List<NotaDTO> Notas { get; set; } = new List<NotaDTO>();
        public decimal PromedioSaber { get; set; }
        public decimal PromedioHacer { get; set; }
        public decimal PromedioSer { get; set; }
    }
}
