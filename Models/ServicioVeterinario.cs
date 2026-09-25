using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Veterinaria.Models
{
    public class ServicioVeterinario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Display(Name = "Precio (Bs)")]
        [Range(0.01, 10000, ErrorMessage = "El precio debe estar entre 0.01 y 10000.")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        // Relación: un servicio puede aparecer en muchas citas.
        public ICollection<Cita> Citas { get; set; }
            = new List<Cita>();
    }
}
