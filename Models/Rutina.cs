using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Training routine. Child entity of the 1:N relation with <see cref="Entrenador"/>
/// (a routine belongs to exactly one trainer via <see cref="EntrenadorId"/>).
/// </summary>
public class Rutina
{
    [BsonId]
    public ObjectId Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El nombre de la rutina solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(600, ErrorMessage = "La descripción no puede superar los 600 caracteres.")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "La descripción no puede ser solo espacios.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dificultad es obligatoria.")]
    public string Dificultad { get; set; } = string.Empty;

    [Range(1, 365, ErrorMessage = "La duración debe estar entre 1 y 365 días.")]
    public int DuracionDias { get; set; } = 30;

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaCreacion { get; set; }

    /// <summary>Foreign key to <see cref="Entrenador"/> (side "1" of the 1:N relation).</summary>
    public ObjectId EntrenadorId { get; set; }

    public bool Activo { get; set; } = true;
}
