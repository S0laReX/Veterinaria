using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Models;

namespace Veterinaria.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ServicioVeterinario> ServiciosVeterinarios { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Cita> Citas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANTE:
            // Identity necesita su propia configuración.
            base.OnModelCreating(modelBuilder);

            /*
             * USUARIO -> MASCOTAS
             *
             * Un usuario puede tener muchas mascotas.
             * Una mascota pertenece a un solo usuario.
             */
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
             * MASCOTA -> CITAS
             *
             * Una mascota puede tener muchas citas.
             */
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Mascota)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MascotaId)
                .OnDelete(DeleteBehavior.Cascade);

            /*
             * SERVICIO -> CITAS
             *
             * Un servicio puede estar asociado a muchas citas.
             *
             * Restrict evita borrar un servicio que ya se encuentra
             * utilizado por una cita.
             */
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.ServicioVeterinario)
                .WithMany(s => s.Citas)
                .HasForeignKey(c => c.ServicioVeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración adicional del precio.
            modelBuilder.Entity<ServicioVeterinario>()
                .Property(s => s.Precio)
                .HasPrecision(10, 2);

            // Valor predeterminado para Estado.
            modelBuilder.Entity<Cita>()
                .Property(c => c.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");

            /*
             * Validación a nivel de SQL Server.
             * Incluso si alguien intenta insertar directamente en la BD,
             * solo se aceptarán estos tres estados.
             */
            modelBuilder.Entity<Cita>()
                .ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Citas_Estado",
                        "[Estado] IN ('Pendiente', 'Atendida', 'Cancelada')"
                    ));
        }
    }
}