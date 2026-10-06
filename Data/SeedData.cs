using GimnasioApp.Models;
using MongoDB.Driver;

namespace GimnasioApp.Data;

/// <summary>
/// Demo data for the assignment defense. Each collection is seeded only when
/// empty, so the method is safe to run on every startup and never overwrites
/// data entered through the UI. All documents are created with Activo = true.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(MongoContext db)
    {
        await SeedSociosAsync(db);
        await SeedFichasMedicasAsync(db);
        await SeedEntrenadoresAsync(db);
        await SeedRutinasAsync(db);
        await SeedActividadesAsync(db);
        await SeedInscripcionesAsync(db);
    }

    private static async Task SeedSociosAsync(MongoContext db)
    {
        if (await db.Socios.CountDocumentsAsync(FilterDefinition<Socio>.Empty) > 0)
        {
            return;
        }

        var socios = new List<Socio>
        {
            new() { Nombre = "Lucía",      Apellido = "Fernández", Dni = "38456721", Email = "lucia.fernandez@gmail.com",   Telefono = "11-5555-0101", FechaNacimiento = new DateTime(1996, 3, 15),  FechaAlta = DateTime.Now.AddDays(-180), Activo = true },
            new() { Nombre = "Martín",     Apellido = "Gómez",     Dni = "35123987", Email = "martin.gomez@gmail.com",      Telefono = "11-5555-0102", FechaNacimiento = new DateTime(1993, 7, 22),  FechaAlta = DateTime.Now.AddDays(-150), Activo = true },
            new() { Nombre = "Valentina",  Apellido = "Ríos",      Dni = "41234567", Email = "valentina.rios@gmail.com",    Telefono = "11-5555-0103", FechaNacimiento = new DateTime(1999, 11, 2), FechaAlta = DateTime.Now.AddDays(-120), Activo = true },
            new() { Nombre = "Diego",      Apellido = "Paz",       Dni = "30987654", Email = "diego.paz@gmail.com",         Telefono = "11-5555-0104", FechaNacimiento = new DateTime(1988, 1, 30),  FechaAlta = DateTime.Now.AddDays(-95),  Activo = true },
            new() { Nombre = "Camila",     Apellido = "Torres",    Dni = "44567890", Email = "camila.torres@gmail.com",     Telefono = "11-5555-0105", FechaNacimiento = new DateTime(2001, 5, 18),  FechaAlta = DateTime.Now.AddDays(-60),  Activo = true },
            new() { Nombre = "Sebastián",  Apellido = "Ruiz",      Dni = "36789012", Email = "sebastian.ruiz@gmail.com",    Telefono = "11-5555-0106", FechaNacimiento = new DateTime(1995, 9, 9),   FechaAlta = DateTime.Now.AddDays(-30),  Activo = true },
        };

        foreach (var socio in socios)
        {
            await db.Socios.InsertOneAsync(socio);
        }
    }

    private static async Task SeedFichasMedicasAsync(MongoContext db)
    {
        if (await db.FichasMedicas.CountDocumentsAsync(FilterDefinition<FichaMedica>.Empty) > 0)
        {
            return;
        }

        // Four of the six seeded socios get a medical record; Camila and
        // Sebastián stay without one so "Crear ficha médica" can be demonstrated.
        var socios = await db.Socios.Find(Builders<Socio>.Filter.Eq(x => x.Activo, true))
            .SortBy(x => x.Dni)
            .Limit(4)
            .ToListAsync();

        var fichas = new List<FichaMedica>
        {
            new() { Peso = 58.5, Altura = 165, TipoSangre = "O+", Alergias = "Ninguna",             Observaciones = "Sin observaciones.",            FechaActualizacion = DateTime.Now.AddDays(-170), Activo = true },
            new() { Peso = 82.3, Altura = 178, TipoSangre = "A-", Alergias = "Polen",               Observaciones = "Asma leve, lleva inhalador.",    FechaActualizacion = DateTime.Now.AddDays(-140), Activo = true },
            new() { Peso = 54.0, Altura = 160, TipoSangre = "B+", Alergias = "Ninguna",             Observaciones = "Entrena 3 veces por semana.",    FechaActualizacion = DateTime.Now.AddDays(-110), Activo = true },
            new() { Peso = 91.2, Altura = 182, TipoSangre = "AB-", Alergias = "Mariscos",           Observaciones = "Hipertensión controlada.",       FechaActualizacion = DateTime.Now.AddDays(-85),  Activo = true },
        };

        for (var i = 0; i < socios.Count; i++)
        {
            fichas[i].SocioId = socios[i].Id;
            await db.FichasMedicas.InsertOneAsync(fichas[i]);
        }
    }

    private static async Task SeedEntrenadoresAsync(MongoContext db)
    {
        if (await db.Entrenadores.CountDocumentsAsync(FilterDefinition<Entrenador>.Empty) > 0)
        {
            return;
        }

        var entrenadores = new List<Entrenador>
        {
            new() { Nombre = "Franco",   Apellido = "Ferrara", Email = "franco.ferrara@gimnasiopro.com",  Telefono = "11-5555-0201", Especialidad = "Fuerza y acondicionamiento", FechaContratacion = DateTime.Now.AddDays(-400), Activo = true },
            new() { Nombre = "Andrea",   Apellido = "Molina",  Email = "andrea.molina@gimnasiopro.com",   Telefono = "11-5555-0202", Especialidad = "Yoga y movilidad",           FechaContratacion = DateTime.Now.AddDays(-300), Activo = true },
            new() { Nombre = "Rodrigo",  Apellido = "Sosa",    Email = "rodrigo.sosa@gimnasiopro.com",    Telefono = "11-5555-0203", Especialidad = "Cardio y HIIT",              FechaContratacion = DateTime.Now.AddDays(-210), Activo = true },
        };

        foreach (var entrenador in entrenadores)
        {
            await db.Entrenadores.InsertOneAsync(entrenador);
        }
    }

    private static async Task SeedRutinasAsync(MongoContext db)
    {
        if (await db.Rutinas.CountDocumentsAsync(FilterDefinition<Rutina>.Empty) > 0)
        {
            return;
        }

        var entrenadores = await db.Entrenadores.Find(Builders<Entrenador>.Filter.Eq(x => x.Activo, true))
            .SortBy(x => x.Apellido)
            .ToListAsync();

        if (entrenadores.Count == 0)
        {
            return;
        }

        // (trainer slot, name, description, difficulty, days, created days ago)
        var plan = new (int EntrenadorSlot, string Nombre, string Descripcion, string Dificultad, int Dias, int Hace)[]
        {
            (0, "Full body para principiantes", "Rutina de entrada con ejercicios compuestos y progresión semanal.",        "Baja", 30, 160),
            (0, "Press banca progresivo",       "Ciclo de fuerza de 6 semanas para aumentar el press banca.",              "Alta", 45, 130),
            (0, "Hipertrofia de piernas",       "Volumen alto para cuádriceps, isquios y glúteos.",                        "Media", 40, 100),
            (1, "Vinyasa flow",                 "Secuencias fluidas de yoga con enfoque en respiración y equilibrio.",     "Media", 21, 120),
            (1, "Movilidad matutina",           "Circuito corto de movilidad articular para empezar el día.",              "Baja", 14, 90),
            (2, "HIIT quema grasa",             "Intervalos de alta intensidad con descansos activos.",                    "Alta", 28, 75),
            (2, "Cardio de iniciación",         "Trote, elíptica y bicicleta en bloques progresivos.",                     "Baja", 30, 45),
        };

        foreach (var (slot, nombre, descripcion, dificultad, dias, hace) in plan)
        {
            await db.Rutinas.InsertOneAsync(new Rutina
            {
                EntrenadorId = entrenadores[slot % entrenadores.Count].Id,
                Nombre = nombre,
                Descripcion = descripcion,
                Dificultad = dificultad,
                DuracionDias = dias,
                FechaCreacion = DateTime.Now.AddDays(-hace),
                Activo = true
            });
        }
    }

    private static async Task SeedActividadesAsync(MongoContext db)
    {
        if (await db.Actividades.CountDocumentsAsync(FilterDefinition<Actividad>.Empty) > 0)
        {
            return;
        }

        var actividades = new List<Actividad>
        {
            // CupoMaximo = 2 and the seed fills it, to demonstrate the "cupo completo" error.
            new() { Nombre = "Cross Training", Categoria = "Funcional", Horario = "07:00 - 08:00", DiaSemana = "Lunes",     CupoMaximo = 2,  Activo = true },
            new() { Nombre = "Yoga",           Categoria = "Bienestar", Horario = "18:00 - 19:00", DiaSemana = "Miércoles", CupoMaximo = 10, Activo = true },
            new() { Nombre = "Boxeo",          Categoria = "Combate",   Horario = "20:00 - 21:00", DiaSemana = "Martes",    CupoMaximo = 8,  Activo = true },
            new() { Nombre = "Spin Bike",      Categoria = "Cardio",    Horario = "06:00 - 06:45", DiaSemana = "Viernes",   CupoMaximo = 6,  Activo = true },
        };

        foreach (var actividad in actividades)
        {
            await db.Actividades.InsertOneAsync(actividad);
        }
    }

    private static async Task SeedInscripcionesAsync(MongoContext db)
    {
        if (await db.Inscripciones.CountDocumentsAsync(FilterDefinition<Inscripcion>.Empty) > 0)
        {
            return;
        }

        var socios = await db.Socios.Find(Builders<Socio>.Filter.Eq(x => x.Activo, true)).SortBy(x => x.Dni).ToListAsync();
        var actividades = await db.Actividades.Find(Builders<Actividad>.Filter.Eq(x => x.Activo, true)).SortBy(x => x.Nombre).ToListAsync();

        if (socios.Count == 0 || actividades.Count == 0)
        {
            return;
        }

        // Resolved by name so alphabetical ordering cannot break the plan.
        // No repeated (socio, actividad) pair; exactly two fill Cross Training (cupo 2).
        var plan = new (int Socio, string Actividad, string Notas, int Hace)[]
        {
            (0, "Cross Training", "Primera clase.",                    55),
            (1, "Cross Training", "Trae zapatillas de cuerda.",        50),
            (0, "Yoga",           "",                                  40),
            (2, "Yoga",           "Prefiere la primera fila.",         35),
            (4, "Yoga",           "",                                  20),
            (3, "Boxeo",          "Entrena para una pelea amateur.",   30),
            (5, "Boxeo",          "",                                  12),
            (1, "Spin Bike",      "Alquiler de calzas incluido.",      25),
            (2, "Spin Bike",      "",                                  15),
        };

        var usados = new HashSet<(int, string)>();
        foreach (var (socioIndex, actividadNombre, notas, hace) in plan)
        {
            var actividadIndex = actividades.FindIndex(a => a.Nombre == actividadNombre);
            if (socioIndex >= socios.Count || actividadIndex < 0)
            {
                continue;
            }

            if (!usados.Add((socioIndex, actividadNombre)))
            {
                continue;
            }

            await db.Inscripciones.InsertOneAsync(new Inscripcion
            {
                SocioId = socios[socioIndex].Id,
                ActividadId = actividades[actividadIndex].Id,
                FechaInscripcion = DateTime.Now.AddDays(-hace),
                Estado = "Activa",
                Notas = notas,
                Activo = true
            });
        }
    }
}
