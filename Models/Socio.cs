using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Gym member. Root entity of the 1:1 relation with <see cref="FichaMedica"/>
/// (one active medical record per member, enforced by a unique index).
/// </summary>
public class Socio
{
    [BsonId]
    public ObjectId Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El nombre solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 80 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El apellido solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener 7 u 8 dígitos, sin puntos ni espacios.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+\-() ]{6,20}$", ErrorMessage = "El teléfono solo puede contener dígitos, espacios, guiones, paréntesis y el símbolo +.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaNacimiento { get; set; } = new DateTime(1995, 1, 1);

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaAlta { get; set; }

    public bool Activo { get; set; } = true;
}
