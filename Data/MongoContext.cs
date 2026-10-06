using GimnasioApp.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GimnasioApp.Data;

/// <summary>
/// Composition root for MongoDB: owns the (singleton) client, exposes one
/// <see cref="IMongoCollection{TDocument}"/> per entity and creates the indexes
/// that back the relations required by the assignment.
/// </summary>
public class MongoContext
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public IMongoCollection<Socio> Socios { get; }
    public IMongoCollection<FichaMedica> FichasMedicas { get; }
    public IMongoCollection<Entrenador> Entrenadores { get; }
    public IMongoCollection<Rutina> Rutinas { get; }
    public IMongoCollection<Actividad> Actividades { get; }
    public IMongoCollection<Inscripcion> Inscripciones { get; }

    public MongoContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDb:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDb:DatabaseName"] ?? "GimnasioDB";

        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(databaseName);

        Socios = Database.GetCollection<Socio>("socios");
        FichasMedicas = Database.GetCollection<FichaMedica>("fichas_medicas");
        Entrenadores = Database.GetCollection<Entrenador>("entrenadores");
        Rutinas = Database.GetCollection<Rutina>("rutinas");
        Actividades = Database.GetCollection<Actividad>("actividades");
        Inscripciones = Database.GetCollection<Inscripcion>("inscripciones");
    }

    /// <summary>
    /// Creates every index used by the app. Safe to call on each startup (idempotent).
    /// </summary>
    public async Task EnsureIndexesAsync()
    {
        // 1:1 GUARANTEE: at most one ACTIVE medical record per member, enforced by MongoDB.
        // The index is partial (Activo == true) so a logically deleted ficha can be created
        // again for the same socio without tripping a duplicate-key error.
        var fichaKeys = Builders<FichaMedica>.IndexKeys.Ascending(x => x.SocioId);
        var fichaOptions = new CreateIndexOptions<FichaMedica>
        {
            Name = "ux_fichas_medicas_socio_unico",
            Unique = true,
            PartialFilterExpression = Builders<FichaMedica>.Filter.Eq(x => x.Activo, true)
        };
        await FichasMedicas.Indexes.CreateOneAsync(new CreateIndexModel<FichaMedica>(fichaKeys, fichaOptions));

        // 1:N and N:N lookup indexes. Deliberately NOT unique: a logically deleted
        // enrollment must never block re-inserting the same socio+actividad pair.
        await Rutinas.Indexes.CreateOneAsync(new CreateIndexModel<Rutina>(
            Builders<Rutina>.IndexKeys.Ascending(x => x.EntrenadorId),
            new CreateIndexOptions<Rutina> { Name = "ix_rutinas_entrenador" }));

        await Inscripciones.Indexes.CreateOneAsync(new CreateIndexModel<Inscripcion>(
            Builders<Inscripcion>.IndexKeys.Ascending(x => x.SocioId),
            new CreateIndexOptions<Inscripcion> { Name = "ix_inscripciones_socio" }));

        await Inscripciones.Indexes.CreateOneAsync(new CreateIndexModel<Inscripcion>(
            Builders<Inscripcion>.IndexKeys.Ascending(x => x.ActividadId),
            new CreateIndexOptions<Inscripcion> { Name = "ix_inscripciones_actividad" }));
    }
}
