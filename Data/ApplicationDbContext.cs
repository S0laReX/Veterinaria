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

        // =========================================================
        // TABLAS
        // =========================================================

        public DbSet<ServicioVeterinario> ServiciosVeterinarios { get; set; }

        public DbSet<Mascota> Mascotas { get; set; }

        public DbSet<Cita> Citas { get; set; }


        // =========================================================
        // CONFIGURACIÓN DEL MODELO
        // =========================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANTE:
            // Identity necesita ejecutar primero su propia configuración.
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // USUARIO -> MASCOTAS
            // =====================================================
            //
            // Un usuario puede tener muchas mascotas.
            // Una mascota pertenece a un solo usuario.
            //
            // Restrict evita que SQL Server intente eliminar
            // automáticamente todas las mascotas al eliminar
            // un usuario.
            //
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // MASCOTA -> CITAS
            // =====================================================
            //
            // Una mascota puede tener muchas citas.
            // Una cita pertenece a una sola mascota.
            //
            // Restrict evita problemas de múltiples rutas
            // de eliminación en cascada.
            //
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Mascota)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MascotaId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // SERVICIO -> CITAS
            // =====================================================
            //
            // Un servicio puede estar asociado a muchas citas.
            //
            // Restrict evita eliminar un servicio que ya está
            // siendo utilizado por una cita.
            //
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.ServicioVeterinario)
                .WithMany(s => s.Citas)
                .HasForeignKey(c => c.ServicioVeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // USUARIO -> CITAS
            // =====================================================
            //
            // Un usuario puede tener muchas citas.
            // Una cita pertenece a un usuario.
            //
            // MUY IMPORTANTE:
            // También utilizamos Restrict aquí para evitar
            // múltiples rutas de eliminación en cascada.
            //
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // PRECIO DEL SERVICIO
            // =====================================================
            //
            // SQL Server almacenará el precio como decimal(10,2).
            //
            modelBuilder.Entity<ServicioVeterinario>()
                .Property(s => s.Precio)
                .HasPrecision(10, 2);


            // =====================================================
            // ESTADO DE LA CITA
            // =====================================================
            //
            // Si no se especifica un estado, será:
            // "Pendiente"
            //
            modelBuilder.Entity<Cita>()
                .Property(c => c.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("Pendiente");


            // =====================================================
            // RESTRICCIÓN DEL ESTADO
            // =====================================================
            //
            // SQL Server solamente permitirá:
            //
            // Pendiente
            // Atendida
            // Cancelada
            //
            modelBuilder.Entity<Cita>()
                .ToTable(table =>
                    table.HasCheckConstraint(
                        "CK_Citas_Estado",
                        "[Estado] IN ('Pendiente', 'Atendida', 'Cancelada')"
                    ));
        }
    }
}