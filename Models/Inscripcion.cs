using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Join collection of the N:N relation between <see cref="Socio"/> and <see cref="Actividad"/>.
/// The rubric requires this module WITHOUT update: entries are created and
/// logically deleted only (there is no edit page for enrollments).
/// </summary>
public class Inscripcion
{
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>Foreign key to <see cref="Socio"/>.</summary>
    public ObjectId SocioId { get; set; }

    /// <summary>Foreign key to <see cref="Actividad"/>.</summary>
    public ObjectId ActividadId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaInscripcion { get; set; }

    /// <summary>"Activa" or "Cancelada". Set by the business layer; not editable by the user (N:N without update).</summary>
    public string Estado { get; set; } = "Activa";

    [StringLength(300, ErrorMessage = "Las notas no pueden superar los 300 caracteres.")]
    [RegularExpression(@"^(?!\s*$)(.*\S.*)?$", ErrorMessage = "Las notas no pueden ser solo espacios.")]
    public string Notas { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
