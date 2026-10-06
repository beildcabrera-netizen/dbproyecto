using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Medical record of a member (1:1 with <see cref="Socio"/>).
/// The unique index on <c>SocioId</c> (partial on Activo == true) guarantees
/// at database level that a member has at most ONE active medical record.
/// </summary>
public class FichaMedica
{
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>Foreign key to <see cref="Socio"/> (side "1" of the 1:1 relation).</summary>
    public ObjectId SocioId { get; set; }

    [Range(20, 400, ErrorMessage = "El peso debe estar entre 20 y 400 kg.")]
    public double Peso { get; set; }

    [Range(50, 250, ErrorMessage = "La altura debe estar entre 50 y 250 cm.")]
    public double Altura { get; set; }

    [Required(ErrorMessage = "El tipo de sangre es obligatorio.")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El tipo de sangre no puede ser solo espacios.")]
    public string TipoSangre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Las alergias no pueden superar los 300 caracteres.")]
    [RegularExpression(@"^(?!\s*$)(.*\S.*)?$", ErrorMessage = "Este campo no puede ser solo espacios.")]
    public string Alergias { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [RegularExpression(@"^(?!\s*$)(.*\S.*)?$", ErrorMessage = "Este campo no puede ser solo espacios.")]
    public string Observaciones { get; set; } = string.Empty;

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaActualizacion { get; set; }

    public bool Activo { get; set; } = true;
}
