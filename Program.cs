using GimnasioApp.Components;
using GimnasioApp.Data;
using GimnasioApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MongoDB wiring. Everything is a singleton: MongoClient is thread-safe by design
// and the services are stateless, so a single instance per circuit/app is enough.
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<SocioService>();
builder.Services.AddSingleton<FichaMedicaService>();
builder.Services.AddSingleton<EntrenadorService>();
builder.Services.AddSingleton<RutinaService>();
builder.Services.AddSingleton<ActividadService>();
builder.Services.AddSingleton<InscripcionService>();

var app = builder.Build();

// Idempotent index creation + demo seed before serving the first request.
try
{
    var mongo = app.Services.GetRequiredService<MongoContext>();
    await mongo.EnsureIndexesAsync();
    await SeedData.SeedAsync(mongo);
}
catch (Exception ex)
{
    Console.Error.WriteLine("ERROR: no se pudo conectar a MongoDB (mongodb://localhost:27017, base GimnasioDB).");
    Console.Error.WriteLine("Verifique que el servicio de MongoDB esté en ejecución antes de iniciar la aplicación.");
    Console.Error.WriteLine($"Detalle: {ex.Message}");
    throw;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
