using ApprFlow.Api.Services.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Endpoints
{
    public static class PlantillaPaso
    {
        internal static void MapPlantillaPasos(this IEndpointRouteBuilder app)
        {
            var plantilla_pasos = app.MapGroup("/api/apprflow/template-step");

            plantilla_pasos.MapGet("/", static async (IServicio<DTO.PlantillaPaso> servicio) =>
                await servicio.Listar());

            plantilla_pasos.MapGet("/{id}", static async Task<Results<Ok<DTO.PlantillaPaso>, NotFound>> (int id, IServicio<DTO.PlantillaPaso> servicio) =>
                await servicio.ListarPorId(id)
                    is DTO.PlantillaPaso plantilla_paso
                        ? TypedResults.Ok<DTO.PlantillaPaso>(plantilla_paso)
                        : TypedResults.NotFound());

            plantilla_pasos.MapGet("/template/{id}", static async (int id, IServicio<DTO.PlantillaPaso> servicio) =>
                await ((Services.PlantillaPaso)servicio).ListarPorPlantillaId(id));

            plantilla_pasos.MapPost("/", static async Task<Results<Created<DTO.PlantillaPaso>, BadRequest<Dominio.Error>>> (
                DTO.PlantillaPaso plantilla_paso, IServicio<DTO.PlantillaPaso> servicio) =>
            {
                // BR: Cada paso dentro de un flujo, tiene un autorizador pre-asignado
                if (!await ((Services.PlantillaPaso)servicio).EsUsuarioValido(plantilla_paso.UsuarioAprobadorId))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"El usuario ID: {plantilla_paso.UsuarioAprobadorId}, ¡no es válido!",
                        "Cada paso dentro del flujo, debe tener un autorizador pre-asignado."
                    ));
                // BR: Cada paso tiene un orden especifico
                // BR: Todos los flujos son secuenciales
                if (!await ((Services.PlantillaPaso)servicio).EsSecuenciaValida(plantilla_paso.Orden, plantilla_paso.PlantillaId))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"La secuencia del paso, ID: {plantilla_paso.Orden}, ¡no es válida!",
                        "Cada paso del flujo, debe tener un orden específico y/o secuencial."
                    ));
                try {
                    plantilla_paso = await servicio.Insertar(plantilla_paso);
                    return TypedResults.Created($"/api/apprflow/template-step/{plantilla_paso.Id}", plantilla_paso);
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
            plantilla_pasos.MapPut("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.PlantillaPaso plantilla_paso, IServicio<DTO.PlantillaPaso> servicio) =>
            {
                return await servicio.Reemplazar(id, plantilla_paso)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
            //  To support partial updates, use HTTP PATCH.
            plantilla_pasos.MapPatch("/{plantilla_id}/{paso_id}", static async Task<Results<NoContent, NotFound>> (
                int plantilla_id, int paso_id, DTO.PlantillaPaso plantilla_paso, IServicio<DTO.PlantillaPaso> servicio) =>
            {
                return await servicio.Actualizar(0, plantilla_paso)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });

            plantilla_pasos.MapDelete("/{id}", static async Task<Results<NoContent, NotFound>> (int id, IServicio<DTO.PlantillaPaso> servicio) =>
            {
                if (await servicio.Eliminar(id))
                    return TypedResults.NoContent();
                return TypedResults.NotFound();
            });
        }
    }
}
