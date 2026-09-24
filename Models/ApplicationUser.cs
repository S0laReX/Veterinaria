using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [StringLength(150)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        // Relación: un usuario puede tener muchas mascotas.
        public ICollection<Mascota> Mascotas { get; set; }
            = new List<Mascota>();
    }
}