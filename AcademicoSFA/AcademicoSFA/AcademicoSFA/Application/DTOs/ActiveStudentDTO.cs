namespace AcademicoSFA.Application.DTOs
{
    public class ActiveStudentDTO
    {
        public required string Codigo { get; set; }
        public required int IdParticipante { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public string? Documento { get; set; }
        public required string Email { get; set; }
        public required string Estado { get; set; }
    }
}
