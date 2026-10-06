using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>
/// CRUD for activities (one side of the N:N relation with <see cref="Socio"/>,
/// joined by the <see cref="Inscripcion"/> collection).
/// Activities themselves are ordinary entities, so they support update.
/// </summary>
public class ActividadService
{
    private readonly MongoContext _db;

    public ActividadService(MongoContext db) => _db = db;

    public async Task<List<Actividad>> GetActivasAsync() =>
        await _db.Actividades.Find(Builders<Actividad>.Filter.Eq(x => x.Activo, true))
            .SortBy(x => x.Nombre)
            .ToListAsync();

    public async Task<Actividad?> GetPorIdAsync(ObjectId id) =>
        await _db.Actividades.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CrearAsync(Actividad actividad)
    {
        actividad.Nombre = TextNormalizer.Collapse(actividad.Nombre);
        actividad.Categoria = TextNormalizer.Collapse(actividad.Categoria);
        actividad.Horario = TextNormalizer.Collapse(actividad.Horario);
        actividad.DiaSemana = TextNormalizer.Collapse(actividad.DiaSemana);
        actividad.Id = ObjectId.GenerateNewId();
        actividad.Activo = true;
        await _db.Actividades.InsertOneAsync(actividad);
    }

    public async Task ActualizarAsync(Actividad actividad)
    {
        actividad.Nombre = TextNormalizer.Collapse(actividad.Nombre);
        actividad.Categoria = TextNormalizer.Collapse(actividad.Categoria);
        actividad.Horario = TextNormalizer.Collapse(actividad.Horario);
        actividad.DiaSemana = TextNormalizer.Collapse(actividad.DiaSemana);
        var result = await _db.Actividades.ReplaceOneAsync(x => x.Id == actividad.Id, actividad);
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException("La actividad ya no existe en la base de datos.");
        }
    }

    /// <summary>Logical delete: sets Activo = false. The document is never removed.</summary>
    public async Task DesactivarAsync(ObjectId id)
    {
        var update = Builders<Actividad>.Update.Set(x => x.Activo, false);
        await _db.Actividades.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task<long> ContarActivasAsync() =>
        await _db.Actividades.CountDocumentsAsync(Builders<Actividad>.Filter.Eq(x => x.Activo, true));
}
