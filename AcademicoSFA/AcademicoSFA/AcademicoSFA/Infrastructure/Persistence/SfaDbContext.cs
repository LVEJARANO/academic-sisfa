using AcademicoSFA.Application.DTOs;
using AcademicoSFA.Domain.Entities;
using AcademicoSFA.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AcademicoSFA.Infrastructure.Data
{
    public class SfaDbContext : DbContext
    {
        public SfaDbContext(DbContextOptions<SfaDbContext> options) : base(options) { }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<AlumnoModel> Alumnos { get; set; }
        public DbSet<Grado> Grados { get; set; }
        public DbSet<GrupoAcad> GruposAcad { get; set; }
        public DbSet<Periodo> Periodos { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<GrupoDocente> GruposDoc { get; set; }
        public DbSet<MatriculaModels> Matriculas { get; set; }
        public DbSet<PagoModel> Pagos { get; set; }
        public DbSet<DetallePagoModels> DetallePagoModels { get; set; }
        public DbSet<DocenteMateriaGrupoAcad> DocenteMateriasGrupoAcad { get; set; }
        public DbSet<GrupoAcadMateria> GrupoAcadMateria { get; set; }
        public DbSet<ActiveStudentDTO> ActiveStudentDTO { get; set; }
        public DbSet<UpdateResult> UpdateResults { get; set; }
        public DbSet<MatriculaInfo> MatriculasInfo { get; set; }
        public DbSet<MatriculaPagosModels> MatriculaPagosModels { get; set; }
        public DbSet<GradosGrupos> GradosGruposInfo { get; set; }
        public DbSet<MateriasADocentesModels> MateriasADocentesModels { get; set; }
        public DbSet<PeriodoAcademico> PeriodoAcademico { get; set; }
        public DbSet<CursoNotas> CursoNotas { get; set; }
        public DbSet<NotaEstudiante> NotaEstudiante { get; set; }
        public DbSet<NotaModels> NotaModels { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participante>().ToTable("TBL_PARTICIPANTE");
            modelBuilder.Entity<AlumnoModel>().ToTable("TBL_ALUMNO");
            modelBuilder.Entity<Grado>().ToTable("TBL_GRADO");
            modelBuilder.Entity<GrupoAcad>().ToTable("TBL_GRUPO_ACAD");
            modelBuilder.Entity<Periodo>().ToTable("TBL_PERIODO");
            modelBuilder.Entity<Materia>().ToTable("TBL_MATERIA");
            modelBuilder.Entity<GrupoDocente>().ToTable("TBL_GRUPO_DOCENTE");
            modelBuilder.Entity<MatriculaModels>().ToTable("TBL_MATRICULA");
            modelBuilder.Entity<PagoModel>().ToTable("TBL_PAGO");
            modelBuilder.Entity<DocenteMateriaGrupoAcad>().ToTable("TBL_DOCENTE_MATERIA_GRUPO_ACAD");
            modelBuilder.Entity<GrupoAcadMateria>().ToTable("TBL_GRUPO_ACAD_MATERIA");
            modelBuilder.Entity<PeriodoAcademico>().ToTable("TBL_PERIODO_ACADEMICO");
            modelBuilder.Entity<NotaModels>().ToTable("TBL_NOTAS");
            modelBuilder.Entity<ActiveStudentDTO>().HasNoKey(); // Configura ActiveStudentDTO como una entidad sin clave
            modelBuilder.Entity<UpdateResult>().HasNoKey();
            modelBuilder.Entity<MatriculaInfo>().HasNoKey();
            modelBuilder.Entity<MatriculaPagosModels>().HasNoKey();
            modelBuilder.Entity<DetallePagoModels>().HasNoKey();
            modelBuilder.Entity<MateriasADocentesModels>().HasNoKey();
            modelBuilder.Entity<NotaEstudiante>().HasNoKey();
            modelBuilder.Entity<CursoNotas>().HasNoKey();
            modelBuilder.Entity<MatriculaModels>()

            .HasMany(m => m.Pagos)
            .WithOne(p => p.Matricula)
            .HasForeignKey(p => p.MatriculaId);



            // Configuración de las relaciones
            modelBuilder.Entity<NotaModels>()
                .HasOne(n => n.Materia)
                .WithMany()
                .HasForeignKey(n => n.IdMateria);

            modelBuilder.Entity<NotaModels>()
                .HasOne(n => n.PeriodoAcademico)
                .WithMany()
                .HasForeignKey(n => n.IdPeriodoAcademico);

            modelBuilder.Entity<NotaModels>()
                .HasOne(n => n.Matricula)
                .WithMany()
                .HasForeignKey(n => n.IdMatricula);

            modelBuilder.Entity<GradosGrupos>().HasNoKey().ToView(null);
        }
        public async Task<List<Materia>> GetAllMateriasAsync()
        {
            return await Materias
                .FromSqlRaw("CALL GetAllMaterias()") 
                .ToListAsync();
        }

        public async Task<List<Materia>> GetMateriasByNombreAsync(string nombreMateria)
        {
            return await Materias
                .FromSqlRaw("CALL GetMateriasByNombre({0})", nombreMateria) 
                .ToListAsync();
        }



    
    }
}
