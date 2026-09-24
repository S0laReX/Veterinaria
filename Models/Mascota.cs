using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria.")]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Raza { get; set; }

        // FK hacia ApplicationUser / AspNetUsers
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public ApplicationUser Usuario { get; set; } = null!;

        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}