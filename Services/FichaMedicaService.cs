using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>
/// CRUD for medical records (1:1 with <see cref="Socio"/>).
/// The service refuses to create a second ACTIVE record for the same member;
/// the database enforces it too through the unique partial index on SocioId.
/// </summary>
public class FichaMedicaService
{
    private readonly MongoContext _db;

    public FichaMedicaService(MongoContext db) => _db = db;

    /// <summary>Returns the member's active medical record, or null when it does not exist yet.</summary>
    public async Task<FichaMedica?> GetActivaPorSocioAsync(ObjectId socioId) =>
        await _db.FichasMedicas.Find(x => x.SocioId == socioId && x.Activo).FirstOrDefaultAsync();

    /// <summary>
    /// Creates the medical record for a member.
    /// Throws when an active record already exists (service-level 1:1 guard).
    /// A logically deleted record for the same member is reactivated instead of duplicated.
    /// </summary>
    public async Task CrearAsync(FichaMedica ficha)
    {
        ficha.TipoSangre = TextNormalizer.Collapse(ficha.TipoSangre);
        ficha.Alergias = TextNormalizer.Collapse(ficha.Alergias);
        ficha.Observaciones = TextNormalizer.Collapse(ficha.Observaciones);
        var existente = await _db.FichasMedicas
            .Find(x => x.SocioId == ficha.SocioId && x.Activo)
            .FirstOrDefaultAsync();

        if (existente is not null)
        {
            throw new InvalidOperationException(
                "El socio ya tiene una ficha médica activa: la relación 1:1 no permite una segunda ficha.");
        }

        ficha.FechaActualizacion = DateTime.Now;
        ficha.Activo = true;

        var inactiva = await _db.FichasMedicas
            .Find(x => x.SocioId == ficha.SocioId)
            .FirstOrDefaultAsync();

        if (inactiva is not null)
        {
            ficha.Id = inactiva.Id;
            await _db.FichasMedicas.ReplaceOneAsync(x => x.Id == ficha.Id, ficha);
        }
        else
        {
            ficha.Id = ObjectId.GenerateNewId();
            await _db.FichasMedicas.InsertOneAsync(ficha);
        }
    }

    public async Task ActualizarAsync(FichaMedica ficha)
    {
        ficha.TipoSangre = TextNormalizer.Collapse(ficha.TipoSangre);
        ficha.Alergias = TextNormalizer.Collapse(ficha.Alergias);
        ficha.Observaciones = TextNormalizer.Collapse(ficha.Observaciones);
        ficha.FechaActualizacion = DateTime.Now;
        var result = await _db.FichasMedicas
            .ReplaceOneAsync(x => x.Id == ficha.Id && x.Activo, ficha);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException("La ficha médica ya no existe.");
        }
    }

    /// <summary>Logical delete: sets Activo = false on the member's medical record.</summary>
    public async Task DesactivarAsync(ObjectId socioId)
    {
        var update = Builders<FichaMedica>.Update.Set(x => x.Activo, false);
        await _db.FichasMedicas.UpdateOneAsync(x => x.SocioId == socioId && x.Activo, update);
    }

    public async Task<long> ContarActivasAsync() =>
        await _db.FichasMedicas.CountDocumentsAsync(Builders<FichaMedica>.Filter.Eq(x => x.Activo, true));
}
