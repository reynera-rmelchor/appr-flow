using ApprFlow.Api.Services.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApprFlow.Api.Endpoints
{
    public static class Flujo
    {
        internal static void MapFlujos(this IEndpointRouteBuilder app)
        {
            var flujos = app.MapGroup("/api/apprflow/flow");

            flujos.MapGet("/", static async (IServicio<DTO.Flujo> servicio) =>
                await servicio.Listar());

            flujos.MapGet("/{id}", static async Task<Results<Ok<DTO.Flujo>, NotFound>> (int id, IServicio<DTO.Flujo> servicio) =>
                await servicio.ListarPorId(id)
                    is DTO.Flujo flujo
                        ? TypedResults.Ok<DTO.Flujo>(flujo)
                        : TypedResults.NotFound());

            flujos.MapGet("/state/{status}", static async (int status, IServicio<DTO.Flujo> servicio) =>
                await ((Services.Flujo)servicio).ListarPorEstado(status));

            flujos.MapPost("/", static async Task<Results<Created<DTO.Flujo>, BadRequest<Dominio.Error>>> (
                DTO.Flujo flujo, IServicio<DTO.Flujo> servicio) =>
            {
                // BR: Los flujos se deben de crear a partir de plantillas
                if (!await ((Services.Flujo)servicio).EsPlantillaValida(flujo.PlantillaId))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"La plantilla ID: {flujo.PlantillaId}, ¡no es válida!",
                        "Los flujos se deben de crear a partir de plantillas existentes y activas."
                    ));
                try {
                    flujo = await servicio.Insertar(flujo);
                    return TypedResults.Created($"/api/apprflow/flow/{flujo.Id}", flujo);
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
            flujos.MapPut("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Flujo flujo, IServicio<DTO.Flujo> servicio) =>
            {
                return await servicio.Reemplazar(id, flujo)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
            //  To support partial updates, use HTTP PATCH.
            flujos.MapPatch("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Flujo flujo, IServicio<DTO.Flujo> servicio) =>
            {
                return await servicio.Actualizar(id, flujo)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });

            flujos.MapDelete("/{id}", static async Task<Results<NoContent, NotFound>> (int id, IServicio<DTO.Flujo> servicio) =>
            {
                if (await servicio.Eliminar(id))
                    return TypedResults.NoContent();
                return TypedResults.NotFound();
            });
        }
    }
}
