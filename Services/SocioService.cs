using System.Text.RegularExpressions;
using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>
/// CRUD for members (socios). Deletion is always logical: only the Activo flag changes.
/// </summary>
public class SocioService
{
    private readonly MongoContext _db;

    public SocioService(MongoContext db) => _db = db;

    /// <summary>Active socios, optionally filtered by name, surname, DNI or email.</summary>
    public async Task<List<Socio>> GetActivosAsync(string? busqueda = null)
    {
        var filter = Builders<Socio>.Filter.Eq(x => x.Activo, true);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var pattern = new BsonRegularExpression(Regex.Escape(busqueda.Trim()), "i");
            var f = Builders<Socio>.Filter;
            filter = f.And(filter, f.Or(
                f.Regex(x => x.Nombre, pattern),
                f.Regex(x => x.Apellido, pattern),
                f.Regex(x => x.Dni, pattern),
                f.Regex(x => x.Email, pattern)));
        }

        return await _db.Socios.Find(filter)
            .SortBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<Socio?> GetPorIdAsync(ObjectId id) =>
        await _db.Socios.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CrearAsync(Socio socio)
    {
        socio.Nombre = TextNormalizer.Collapse(socio.Nombre);
        socio.Apellido = TextNormalizer.Collapse(socio.Apellido);
        socio.Dni = TextNormalizer.Collapse(socio.Dni);
        socio.Email = TextNormalizer.Collapse(socio.Email);
        socio.Telefono = TextNormalizer.Collapse(socio.Telefono);
        socio.Id = ObjectId.GenerateNewId();
        socio.Activo = true;
        socio.FechaAlta = DateTime.Now;
        await _db.Socios.InsertOneAsync(socio);
    }

    public async Task ActualizarAsync(Socio socio)
    {
        socio.Nombre = TextNormalizer.Collapse(socio.Nombre);
        socio.Apellido = TextNormalizer.Collapse(socio.Apellido);
        socio.Dni = TextNormalizer.Collapse(socio.Dni);
        socio.Email = TextNormalizer.Collapse(socio.Email);
        socio.Telefono = TextNormalizer.Collapse(socio.Telefono);
        var result = await _db.Socios.ReplaceOneAsync(x => x.Id == socio.Id, socio);
        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException("El socio ya no existe en la base de datos.");
        }
    }

    /// <summary>Logical delete: sets Activo = false. The document is never removed.</summary>
    public async Task DesactivarAsync(ObjectId id)
    {
        var update = Builders<Socio>.Update.Set(x => x.Activo, false);
        await _db.Socios.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task<long> ContarActivosAsync() =>
        await _db.Socios.CountDocumentsAsync(Builders<Socio>.Filter.Eq(x => x.Activo, true));
}
