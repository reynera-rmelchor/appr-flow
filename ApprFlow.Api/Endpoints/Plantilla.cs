using ApprFlow.Api.Services.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Endpoints
{
    public static class Plantilla
    {
        internal static void MapPlantillas(this IEndpointRouteBuilder app)
        {
            var plantillas = app.MapGroup("/api/apprflow/template");

            plantillas.MapGet("/", static async (IServicio<DTO.Plantilla> servicio) =>
                await servicio.Listar());

            plantillas.MapGet("/{id}", static async Task<Results<Ok<DTO.Plantilla>, NotFound>> (int id, IServicio<DTO.Plantilla> servicio) =>
                await servicio.ListarPorId(id)
                    is DTO.Plantilla plantilla
                        ? TypedResults.Ok<DTO.Plantilla>(plantilla)
                        : TypedResults.NotFound());

            plantillas.MapGet("/active/{status}", static async (bool status, IServicio<DTO.Plantilla> servicio) =>
                await ((Services.Plantilla)servicio).ListarPorEstado(status));
            
            plantillas.MapPost("/", static async Task<Results<Created<DTO.Plantilla>, BadRequest<Dominio.Error>>> (
                DTO.Plantilla plantilla, IServicio<DTO.Plantilla> servicio) =>
            {   
                try {
                    plantilla = await servicio.Insertar(plantilla);
                    return TypedResults.Created($"/api/apprflow/template/{plantilla.Id}", plantilla);
                } catch (Exception ex) {
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.INS),
                        $"{ex.Message}",
                        $"{ex.InnerException?.Message}"
                    ));
                }
            });
            // According to the HTTP specification, a PUT request requires
            //  the client to send the entire updated entity, not just the changes.
            plantillas.MapPut("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Plantilla plantilla, IServicio<DTO.Plantilla> servicio) =>
            {
                return await servicio.Reemplazar(id, plantilla)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
            //  To support partial updates, use HTTP PATCH.
            plantillas.MapPatch("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Plantilla plantilla, IServicio<DTO.Plantilla> servicio) =>
            {
                return await servicio.Actualizar(id, plantilla)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });

            plantillas.MapDelete("/{id}", static async Task<Results<NoContent, NotFound>> (int id, IServicio<DTO.Plantilla> servicio) =>
            {
                if (await servicio.Eliminar(id))
                    return TypedResults.NoContent();
                return TypedResults.NotFound();
            });
        }
    }
}
