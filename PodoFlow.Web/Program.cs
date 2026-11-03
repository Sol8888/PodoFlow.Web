
using PodoFlow.Web.Components;
using PodoFlow.Web.Seguridad;
using PodoFlow.Web.Servicios;

var builder = WebApplication.CreateBuilder(args);

// SEGURIDAD

// Handler de autenticación para desarrollo
builder.Services.AddTransient<TokenDesarrolloHandler>();

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
