using Microsoft.EntityFrameworkCore;
using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Endpoints;
using ApprFlow.Api.Services.Core;

var builder = WebApplication.CreateBuilder(args);

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<ContextoBD>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Registrar la lógica de negocio
builder.Services.AddScoped<IServicio<ApprFlow.Api.DTO.Usuario>, ApprFlow.Api.Services.Usuario>();
builder.Services.AddScoped<IServicio<ApprFlow.Api.DTO.Plantilla>, ApprFlow.Api.Services.Plantilla>();
builder.Services.AddScoped<IServicio<ApprFlow.Api.DTO.PlantillaPaso>, ApprFlow.Api.Services.PlantillaPaso>();
builder.Services.AddScoped<IServicio<ApprFlow.Api.DTO.Flujo>, ApprFlow.Api.Services.Flujo>();
builder.Services.AddScoped<IServicio<ApprFlow.Api.DTO.FlujoPaso>, ApprFlow.Api.Services.FlujoPaso>();
// Ignorar referencias de bucle al serializar objetos a JSON
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => {
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Registrar AutoMapper y buscar perfiles de mapeo en la solución
builder.Services.AddAutoMapper(cfg => {
    cfg.AddMaps(typeof(Program).Assembly);
});
// Configurar la generación del documento OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Exponer el JSON de OpenAPI
    app.MapOpenApi();
    // Activar la interfaz gráfica de Swagger UI apuntando al JSON
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "ApprFlow API v1");
    });
}

// Redirigir automáticamente las solicitudes HTTP a HTTPS
app.UseHttpsRedirection();

// Mapear los endpoints
app.MapUsuarios();
app.MapPlantillas();
app.MapPlantillaPasos();
app.MapFlujos();
app.MapFlujoPasos();

app.Run();
