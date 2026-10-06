using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>
/// CRUD for trainers (parent side of the 1:N relation with <see cref="Rutina"/>).
/// Deletion is logical: only the Activo flag changes.
/// </summary>
public class EntrenadorService
{
    private readonly MongoContext _db;

    public EntrenadorService(MongoContext db) => _db = db;

    public async Task<List<Entrenador>> GetActivosAsync() =>
        await _db.Entrenadores.Find(Builders<Entrenador>.Filter.Eq(x => x.Activo, true))
            .SortBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();

    public async Task<Entrenador?> GetPorIdAsync(ObjectId id) =>
        await _db.Entrenadores.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CrearAsync(Entrenador entrenador)
    {
        entrenador.Nombre = TextNormalizer.Collapse(entrenador.Nombre);
        entrenador.Apellido = TextNormalizer.Collapse(entrenador.Apellido);
        entrenador.Email = TextNormalizer.Collapse(entrenador.Email);
        entrenador.Telefono = TextNormalizer.Collapse(entrenador.Telefono);
        entrenador.Especialidad = TextNormalizer.Collapse(entrenador.Especialidad);
        entrenador.Id = ObjectId.GenerateNewId();
        entrenador.Activo = true;
        await _db.Entrenadores.InsertOneAsync(entrenador);
    }

    public async Task ActualizarAsync(Entrenador entrenador)
    {
        entrenador.Nombre = TextNormalizer.Collapse(entrenador.Nombre);
        entrenador.Apellido = TextNormalizer.Collapse(entrenador.Apellido);
        entrenador.Email = TextNormalizer.Collapse(entrenador.Email);
        entrenador.Telefono = TextNormalizer.Collapse(entrenador.Telefono);
        entrenador.Especialidad = TextNormalizer.Collapse(entrenador.Especialidad);
        var result = await _db.Entrenadores.ReplaceOneAsync(x => x.Id == entrenador.Id, entrenador);
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException("El entrenador ya no existe en la base de datos.");
        }
    }

    /// <summary>Logical delete: sets Activo = false. The document is never removed.</summary>
    public async Task DesactivarAsync(ObjectId id)
    {
        var update = Builders<Entrenador>.Update.Set(x => x.Activo, false);
        await _db.Entrenadores.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task<long> ContarActivosAsync() =>
        await _db.Entrenadores.CountDocumentsAsync(Builders<Entrenador>.Filter.Eq(x => x.Activo, true));
}
