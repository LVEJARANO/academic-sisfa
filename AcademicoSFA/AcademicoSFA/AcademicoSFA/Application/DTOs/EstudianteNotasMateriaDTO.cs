namespace AcademicoSFA.Application.DTOs
{
    public class EstudianteNotasMateriaDTO
    {
        public int IdMatricula { get; set; }
        public string Codigo { get; set; }
        public string NombreCompleto { get; set; }
        public List<NotaDTO> Notas { get; set; } = new List<NotaDTO>();
        public decimal PromedioSaber { get; set; }
        public decimal PromedioHacer { get; set; }
        public decimal PromedioSer { get; set; }
        public decimal PromedioFinal { get; set; }
    }
}
