using PodoFlow.Web.Components;
using PodoFlow.Web.Demo;
using PodoFlow.Web.Seguridad;
using PodoFlow.Web.Servicios;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddTransient<TokenDesarrolloHandler>();

builder.Services.AddHttpClient<PacientesApi>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "No se configuró la URL de PodoFlow.Api."));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpClient<PrivacidadApi>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "No se configuró la URL de PodoFlow.Api."));
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<TokenDesarrolloHandler>();

if (builder.Configuration.GetValue<bool>("Privacidad:ModoDemo"))
    builder.Services.AddSingleton<IPrivacidadApi, PrivacidadApiDemo>();
else
    builder.Services.AddTransient<IPrivacidadApi>(sp => sp.GetRequiredService<PrivacidadApi>());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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
