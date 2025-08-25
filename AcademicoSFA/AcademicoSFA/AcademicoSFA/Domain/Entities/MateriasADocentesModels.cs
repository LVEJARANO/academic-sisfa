namespace AcademicoSFA.Domain.Entities
{
    public class MateriasADocentesModels
    {
        public string Nombres { get; set; }

        public string? Documento { get; set; }
        public string Correo { get; set; }
        public string Grado { get; set; }
        public string Materia { get; set; }
        public int Id { get; set; }
        public int IdMateria { get; set; }
        public int IdParticipante { get; set; }
    }
}
