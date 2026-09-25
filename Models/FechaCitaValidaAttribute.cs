using System.ComponentModel.DataAnnotations;

namespace Veterinaria.Models
{
    public class FechaCitaValidaAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not DateTime fecha)
                return false;

            return fecha >= DateTime.Now;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} no puede ser anterior a la fecha y hora actual.";
        }
    }
}