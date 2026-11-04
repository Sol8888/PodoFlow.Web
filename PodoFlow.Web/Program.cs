
using PodoFlow.Web.Components;
using PodoFlow.Web.Seguridad;
using PodoFlow.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

// SEGURIDAD

// Handler de autenticación para desarrollo
builder.Services.AddTransient<TokenDesarrolloHandler>();
Console.WriteLine("========================================");
Console.WriteLine($"PROYECTO WEB: {builder.Environment.ContentRootPath}");
Console.WriteLine($"CARPETA WWWROOT: {builder.Environment.WebRootPath}");
Console.WriteLine($"ENTORNO: {builder.Environment.EnvironmentName}");
Console.WriteLine("========================================");

// CONFIGURACIÓN DE API

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "No se configuró la URL de PodoFlow.Api.");

// SERVICIO DE PACIENTES


builder.Services.AddHttpClient<IPacientesApi, PacientesApi>(
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<TokenDesarrolloHandler>();


// SERVICIO DE PRIVACIDAD


builder.Services.AddHttpClient<IPrivacidadApi, PrivacidadApi>(
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddHttpMessageHandler<TokenDesarrolloHandler>();


// COMPONENTES BLAZOR


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



// CONSTRUCCIÓN DE LA APLICACIÓN

var app = builder.Build();


var archivoCss = app.Environment.WebRootFileProvider
    .GetFileInfo("app.css");

Console.WriteLine("========== DIAGNÓSTICO CSS ==========");
Console.WriteLine($"ContentRoot: {app.Environment.ContentRootPath}");
Console.WriteLine($"WebRoot: {app.Environment.WebRootPath}");
Console.WriteLine($"Archivo servido: {archivoCss.PhysicalPath}");
Console.WriteLine($"Existe: {archivoCss.Exists}");

if (archivoCss.Exists)
{
    using var stream = archivoCss.CreateReadStream();
    using var lector = new StreamReader(stream);

    var contenido = lector.ReadToEnd();

    Console.WriteLine($"Caracteres: {contenido.Length}");

    Console.WriteLine(
        $"Contiene pf-clinic-record: " +
        $"{contenido.Contains(".pf-clinic-record", StringComparison.Ordinal)}");
}

Console.WriteLine("=====================================");


// MIDDLEWARE


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Error",
        createScopeForErrors: true);

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();


// MAPEO DE COMPONENTES

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
