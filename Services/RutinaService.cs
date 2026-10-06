using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>
/// CRUD for routines (child side of the 1:N relation with <see cref="Entrenador"/>).
/// Deletion is logical: only the Activo flag changes.
/// </summary>
public class RutinaService
{
    private readonly MongoContext _db;

    public RutinaService(MongoContext db) => _db = db;

    /// <summary>Active routines, optionally filtered by trainer (the 1:N child list).</summary>
    public async Task<List<Rutina>> GetActivasAsync(ObjectId? entrenadorId = null)
    {
        var f = Builders<Rutina>.Filter;
        var filter = f.Eq(x => x.Activo, true);

        if (entrenadorId is not null)
        {
            filter = f.And(filter, f.Eq(x => x.EntrenadorId, entrenadorId.Value));
        }

        return await _db.Rutinas.Find(filter)
            .SortBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<List<Rutina>> GetActivasPorEntrenadorAsync(ObjectId entrenadorId) =>
        await GetActivasAsync(entrenadorId);

    public async Task<Rutina?> GetPorIdAsync(ObjectId id) =>
        await _db.Rutinas.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CrearAsync(Rutina rutina)
    {
        rutina.Nombre = TextNormalizer.Collapse(rutina.Nombre);
        rutina.Descripcion = TextNormalizer.Collapse(rutina.Descripcion);
        rutina.Dificultad = TextNormalizer.Collapse(rutina.Dificultad);
        rutina.Id = ObjectId.GenerateNewId();
        rutina.Activo = true;
        rutina.FechaCreacion = DateTime.Now;
        await _db.Rutinas.InsertOneAsync(rutina);
    }

    public async Task ActualizarAsync(Rutina rutina)
    {
        rutina.Nombre = TextNormalizer.Collapse(rutina.Nombre);
        rutina.Descripcion = TextNormalizer.Collapse(rutina.Descripcion);
        rutina.Dificultad = TextNormalizer.Collapse(rutina.Dificultad);
        var result = await _db.Rutinas.ReplaceOneAsync(x => x.Id == rutina.Id, rutina);
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException("La rutina ya no existe en la base de datos.");
        }
    }

    /// <summary>Logical delete: sets Activo = false. The document is never removed.</summary>
    public async Task DesactivarAsync(ObjectId id)
    {
        var update = Builders<Rutina>.Update.Set(x => x.Activo, false);
        await _db.Rutinas.UpdateOneAsync(x => x.Id == id, update);
    }

    /// <summary>Active routine count per trainer (feeds the 1:N list and the dashboard).</summary>
    public async Task<Dictionary<ObjectId, int>> ContarActivasPorEntrenadorAsync()
    {
        var rutinas = await _db.Rutinas.Find(Builders<Rutina>.Filter.Eq(x => x.Activo, true)).ToListAsync();
        return rutinas
            .GroupBy(r => r.EntrenadorId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<long> ContarActivasAsync() =>
        await _db.Rutinas.CountDocumentsAsync(Builders<Rutina>.Filter.Eq(x => x.Activo, true));
}
