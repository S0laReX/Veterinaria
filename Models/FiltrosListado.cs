using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Veterinaria.Models;

public class FiltrosListado : IValidatableObject
{
    [StringLength(100)] public string? Buscar { get; set; }
    public string? Estado { get; set; }
    public string? Especie { get; set; }
    public string? Sexo { get; set; }
    [DataType(DataType.Date)] public DateTime? Desde { get; set; }
    [DataType(DataType.Date)] public DateTime? Hasta { get; set; }
    [Range(0, 10000, ErrorMessage = "El precio mínimo debe estar entre 0 y 10000 Bs.")]
    public decimal? PrecioMin { get; set; }
    [Range(0, 10000, ErrorMessage = "El precio máximo debe estar entre 0 y 10000 Bs.")]
    public decimal? PrecioMax { get; set; }
    public string? Orden { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Desde.HasValue && Hasta.HasValue && Desde.Value.Date > Hasta.Value.Date)
            yield return new ValidationResult("La fecha inicial debe ser anterior o igual a la final.", [nameof(Hasta)]);
        if (PrecioMin.HasValue && PrecioMax.HasValue && PrecioMin > PrecioMax)
            yield return new ValidationResult("El precio mínimo no puede superar al máximo.", [nameof(PrecioMax)]);
    }

    public IQueryable<ServicioVeterinario> AplicarServicios(IQueryable<ServicioVeterinario> query)
    {
        var texto = Buscar?.Trim();
        if (!string.IsNullOrWhiteSpace(texto))
            query = query.Where(s => s.Nombre.Contains(texto) || s.Descripcion.Contains(texto));
        if (PrecioMin.HasValue) query = query.Where(s => s.Precio >= PrecioMin.Value);
        if (PrecioMax.HasValue) query = query.Where(s => s.Precio <= PrecioMax.Value);
        return Orden switch
        {
            "precio-asc" => query.OrderBy(s => s.Precio).ThenBy(s => s.Nombre),
            "precio-desc" => query.OrderByDescending(s => s.Precio).ThenBy(s => s.Nombre),
            "nombre-desc" => query.OrderByDescending(s => s.Nombre),
            _ => query.OrderBy(s => s.Nombre)
        };
    }
}

public static class Moneda
{
    public static string Bolivianos(decimal precio) => $"Bs {precio.ToString("N2", CultureInfo.GetCultureInfo("es-BO"))}";
}
