using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es obligatoria.")]
        [Display(Name = "Fecha de la cita")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCita { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        [RegularExpression(
            "^(Pendiente|Atendida|Cancelada)$",
            ErrorMessage = "El estado debe ser Pendiente, Atendida o Cancelada.")]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        [Display(Name = "Mascota")]
        public int MascotaId { get; set; }

        public Mascota Mascota { get; set; } = null!;

        [Required]
        [Display(Name = "Servicio veterinario")]
        public int ServicioVeterinarioId { get; set; }

        public ServicioVeterinario ServicioVeterinario { get; set; } = null!;
    }
}