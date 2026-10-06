using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Gym activity. One side of the N:N relation with <see cref="Socio"/>,
/// joined through the <see cref="Inscripcion"/> collection.
/// </summary>
public class Actividad
{
    [BsonId]
    public ObjectId Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El nombre de la actividad solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(60, ErrorMessage = "La categoría no puede superar los 60 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "La categoría solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El horario es obligatorio.")]
    [RegularExpression(@"^(?:[01]\d|2[0-3]):[0-5]\d\s*-\s*(?:[01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "El horario debe tener el formato HH:MM - HH:MM con horas válidas (00-23) y minutos válidos (00-59) (ej.: 07:00 - 08:00).")]
    public string Horario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El día de la semana es obligatorio.")]
    public string DiaSemana { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "El cupo máximo debe estar entre 1 y 500.")]
    public int CupoMaximo { get; set; } = 10;

    public bool Activo { get; set; } = true;
}
