using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria.")]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [Required(ErrorMessage = "La raza es obligatoria.")]
        [StringLength(100)]
        public string Raza { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(0, 100, ErrorMessage = "La edad debe estar entre 0 y 100 años.")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El sexo es obligatorio.")]
        [StringLength(20)]
        public string Sexo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Usuario propietario
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public ApplicationUser? Usuario { get; set; }

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}