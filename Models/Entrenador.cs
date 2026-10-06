using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GimnasioApp.Models;

/// <summary>
/// Trainer. Parent entity of the 1:N relation with <see cref="Rutina"/>
/// (one trainer has many routines, each routine references one trainer).
/// </summary>
public class Entrenador
{
    [BsonId]
    public ObjectId Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 80 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El nombre del entrenador solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 80 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "El apellido del entrenador solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+\-() ]{6,20}$", ErrorMessage = "El teléfono solo puede contener dígitos, espacios, guiones, paréntesis y el símbolo +.")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La especialidad no puede superar los 100 caracteres.")]
    [RegularExpression(@"^(?=.*\S)[A-Za-zÁÉÍÓÚÜÑáéíóúüñ'’\- ]+$", ErrorMessage = "La especialidad solo puede contener letras, espacios, apóstrofes y guiones; y no puede ser solo espacios.")]
    public string Especialidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de contratación es obligatoria.")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime FechaContratacion { get; set; } = DateTime.Today;

    public bool Activo { get; set; } = true;
}
