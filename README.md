# GimnasioPro — Sistema de gestión de gimnasio

Aplicación ASP.NET Core **Blazor Server** (.NET 8) con **MongoDB** para la materia *Base de Datos II*. Gestiona socios, fichas médicas, entrenadores, rutinas, actividades e inscripciones, aplicando los tres tipos de relación del programa.

## Requisitos

- .NET 8 SDK
- MongoDB 7 corriendo en `mongodb://localhost:27017` (el servicio debe estar activo)

No se usan Docker, Atlas, servicios externos ni recursos por CDN: las tipografías y el CSS son locales.

## Ejecución

```bash
dotnet run --project GimnasioApp
```

Al iniciar, la aplicación:

1. Crea los índices de la base `GimnasioDB`, incluido el **índice único parcial** sobre `fichas_medicas.SocioId` (`Activo = true`), que garantiza la relación 1:1 a nivel de base de datos y permite la eliminación lógica + reactivación.
2. Si las colecciones están vacías, inserta datos de ejemplo (6 socios, 4 fichas médicas, 3 entrenadores, 7 rutinas, 4 actividades y 9 inscripciones).

Si MongoDB no está disponible, la aplicación muestra un error en consola y se detiene.

## Módulos y mapeo con la rúbrica

| Requisito | Dónde está |
|---|---|
| **1:1 socios ↔ fichas médicas** | `Components/Pages/Socios/` (lista, crear, detalle, editar) y `Components/Pages/FichasMedicas/Editar.razor`; `Services/FichaMedicaService.cs` impide una segunda ficha activa; el índice único parcial lo refuerza en MongoDB |
| **1:N entrenadores → rutinas** | `Components/Pages/Entrenadores/` y `Components/Pages/Rutinas/`; `Services/RutinaService.cs` (`EntrenadorId` como clave foránea lógica) |
| **N:N socios ↔ actividades sin actualización** | `Components/Pages/Inscripciones/` (solo **Lista** y **Crear**, sin página de edición); `Services/InscripcionService.cs` valida duplicados activos y cupo máximo; `Inscripcion` referencia `SocioId` + `ActividadId` |
| **Eliminación lógica** | Todos los servicios usan `UpdateOneAsync` con `Activo = false`; no existe ningún `DeleteOne`/`DeleteMany` en el proyecto |
| **≥ 6 atributos por colección** | `Models/*.cs`: `Socio`, `FichaMedica`, `Entrenador`, `Rutina`, `Actividad`, `Inscripcion` (además de `Id` y `Activo`) |
| **Interfaz en español** | Todo el texto visible, validaciones y mensajes de error de las páginas `Components/Pages/**` |

## Estructura

```
GimnasioApp/
├── Models/                  # Entidades BSON con validación (DataAnnotations)
├── Data/
│   ├── MongoContext.cs      # Colecciones + EnsureIndexesAsync (índice único parcial 1:1)
│   └── SeedData.cs          # Semilla sólo en colecciones vacías
├── Services/                # CRUD + reglas de negocio + borrado lógico
├── Components/
│   ├── Pages/               # Socios, FichasMedicas, Entrenadores, Rutinas, Actividades, Inscripciones
│   └── Layout/              # Shell con sidebar y navegación por módulos
├── Program.cs               # DI (singleton), índices y semilla antes de arrancar
└── appsettings.json         # MongoDb:Connection string y nombre de base
```

## Decisiones técnicas

- **Índice único parcial** en `fichas_medicas.SocioId` con `PartialFilterExpression = Activo == true`: la base de datos garantiza el 1:1 entre activos y, al mismo tiempo, permite desactivar una ficha y volver a crearla.
- **Sin índice único en inscripciones**: como el borrado es lógico, un índice único compuesto impediría re-inscribirse; las reglas de duplicado y de cupo se validan en `InscripcionService.CrearAsync` con mensajes en español.
- **Modo interactivo global** (`<Routes @rendermode="InteractiveServer" />` en `App.razor`): las páginas no declaran `@rendermode` propio.
- **Eliminación sólo lógica**: ninguna colección se modifica con operaciones de borrado físico.
