using GimnasioApp.Data;
using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Services;

/// <summary>Read model used by the enrollment list: enrollment + denormalized names.</summary>
public class InscripcionDetalle
{
    public Inscripcion Inscripcion { get; init; } = new();
    public string NombreSocio { get; init; } = string.Empty;
    public string DniSocio { get; init; } = string.Empty;
    public string NombreActividad { get; init; } = string.Empty;

    /// <summary>Capacity badge of the activity, e.g. "3/10".</summary>
    public string Cupo { get; init; } = string.Empty;
}

/// <summary>
/// CRUD for the N:N join collection (socios ↔ actividades) WITHOUT update,
/// as required by the rubric: entries can be created and logically deleted only.
/// Business rules enforced here (not by an index, because logical deletes
/// would make a unique compound index block re-enrollment):
///   - no two ACTIVE enrollments for the same socio + actividad,
///   - an enrollment cannot exceed the activity's CupoMaximo.
/// </summary>
public class InscripcionService
{
    private readonly MongoContext _db;

    public InscripcionService(MongoContext db) => _db = db;

    /// <summary>Active enrollments with names resolved in memory (small data set).</summary>
    public async Task<List<InscripcionDetalle>> GetDetallesAsync(ObjectId? socioId = null, ObjectId? actividadId = null)
    {
        var f = Builders<Inscripcion>.Filter;
        var filter = f.Eq(x => x.Activo, true);

        if (socioId is not null)
        {
            filter = f.And(filter, f.Eq(x => x.SocioId, socioId.Value));
        }

        if (actividadId is not null)
        {
            filter = f.And(filter, f.Eq(x => x.ActividadId, actividadId.Value));
        }

        var inscripciones = await _db.Inscripciones.Find(filter)
            .SortByDescending(x => x.FechaInscripcion)
            .ToListAsync();

        if (inscripciones.Count == 0)
        {
            return new List<InscripcionDetalle>();
        }

        var socios = await _db.Socios
            .Find(Builders<Socio>.Filter.Eq(x => x.Activo, true))
            .ToListAsync();
        var actividades = await _db.Actividades
            .Find(Builders<Actividad>.Filter.Eq(x => x.Activo, true))
            .ToListAsync();

        var sociosPorId = socios.ToDictionary(s => s.Id);
        var actividadesPorId = actividades.ToDictionary(a => a.Id);
        var inscritosPorActividad = await ContarPorActividadAsync();

        var detalles = new List<InscripcionDetalle>(inscripciones.Count);
        foreach (var inscripcion in inscripciones)
        {
            var socio = sociosPorId.TryGetValue(inscripcion.SocioId, out var s) ? s : null;
            var actividad = actividadesPorId.TryGetValue(inscripcion.ActividadId, out var a) ? a : null;
            var inscritos = inscritosPorActividad.TryGetValue(inscripcion.ActividadId, out var c) ? c : 0;

            detalles.Add(new InscripcionDetalle
            {
                Inscripcion = inscripcion,
                NombreSocio = socio is null ? "(socio no disponible)" : $"{socio.Nombre} {socio.Apellido}",
                DniSocio = socio?.Dni ?? string.Empty,
                NombreActividad = actividad?.Nombre ?? "(actividad no disponible)",
                Cupo = actividad is null ? "-" : $"{inscritos}/{actividad.CupoMaximo}"
            });
        }

        return detalles;
    }

    /// <summary>
    /// Creates an enrollment after validating the two business rules of the N:N module.
    /// Every rejection message is ready to show to the user.
    /// </summary>
    public async Task CrearAsync(ObjectId socioId, ObjectId actividadId, string? notas)
    {
        var socio = await _db.Socios
            .Find(x => x.Id == socioId && x.Activo)
            .FirstOrDefaultAsync();
        if (socio is null)
        {
            throw new InvalidOperationException("El socio seleccionado no existe o está desactivado.");
        }

        var actividad = await _db.Actividades
            .Find(x => x.Id == actividadId && x.Activo)
            .FirstOrDefaultAsync();
        if (actividad is null)
        {
            throw new InvalidOperationException("La actividad seleccionada no existe o está desactivada.");
        }

        // (a) no duplicate ACTIVE enrollment for the same socio + actividad.
        var duplicada = await _db.Inscripciones
            .CountDocumentsAsync(x => x.SocioId == socioId && x.ActividadId == actividadId && x.Activo) > 0;
        if (duplicada)
        {
            throw new InvalidOperationException(
                $"{socio.Nombre} {socio.Apellido} ya está inscripto/a en \"{actividad.Nombre}\". La inscripción duplicada no está permitida.");
        }

        // (b) the activity must not be full.
        var inscritos = await _db.Inscripciones
            .CountDocumentsAsync(x => x.ActividadId == actividadId && x.Activo);
        if (inscritos >= actividad.CupoMaximo)
        {
            throw new InvalidOperationException(
                $"La actividad \"{actividad.Nombre}\" está completa: cupo máximo {actividad.CupoMaximo} inscriptos/as.");
        }

        await _db.Inscripciones.InsertOneAsync(new Inscripcion
        {
            Id = ObjectId.GenerateNewId(),
            SocioId = socioId,
            ActividadId = actividadId,
            FechaInscripcion = DateTime.Now,
            Estado = "Activa",
            Notas = TextNormalizer.Collapse(notas),
            Activo = true
        });
    }

    /// <summary>Logical delete: sets Activo = false. No document is ever removed.</summary>
    public async Task DesactivarAsync(ObjectId id)
    {
        var update = Builders<Inscripcion>.Update.Set(x => x.Activo, false);
        await _db.Inscripciones.UpdateOneAsync(x => x.Id == id, update);
    }

    /// <summary>Active enrollments per activity (capacity display).</summary>
    public async Task<Dictionary<ObjectId, long>> ContarPorActividadAsync()
    {
        var activas = await _db.Inscripciones
            .Find(Builders<Inscripcion>.Filter.Eq(x => x.Activo, true))
            .ToListAsync();

        return activas
            .GroupBy(i => i.ActividadId)
            .ToDictionary(g => g.Key, g => g.LongCount());
    }

    public async Task<long> ContarActivasAsync() =>
        await _db.Inscripciones.CountDocumentsAsync(Builders<Inscripcion>.Filter.Eq(x => x.Activo, true));
}
