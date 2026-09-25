using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una mascota.")]
        [Display(Name = "Mascota")]
        public int MascotaId { get; set; }

        public Mascota? Mascota { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un servicio.")]
        [Display(Name = "Servicio")]
        public int ServicioVeterinarioId { get; set; }

        public ServicioVeterinario? ServicioVeterinario { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una fecha y hora.")]
        [FechaCitaValida(ErrorMessage = "La fecha de la cita no puede ser anterior a la fecha y hora actual.")]
        [Display(Name = "Fecha y hora")]
        [DataType(DataType.DateTime)]
        public DateTime FechaCita { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(20)]
        [RegularExpression(
            "^(Pendiente|Atendida|Cancelada)$",
            ErrorMessage = "El estado debe ser Pendiente, Atendida o Cancelada.")]
        public string Estado { get; set; } = "Pendiente";

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public ApplicationUser? Usuario { get; set; }
    }
}