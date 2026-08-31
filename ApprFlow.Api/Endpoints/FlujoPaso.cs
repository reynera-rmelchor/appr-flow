using ApprFlow.Api.Services.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Endpoints
{
    public static class FlujoPaso
    {
        internal static void MapFlujoPasos(this IEndpointRouteBuilder app)
        {
            var flujo_pasos = app.MapGroup("/api/apprflow/flow-step");

            flujo_pasos.MapGet("/", static async (IServicio<DTO.FlujoPaso> servicio) =>
                await servicio.Listar());

            flujo_pasos.MapGet("/{id}", static async Task<Results<Ok<DTO.FlujoPaso>, NotFound>> (int id, IServicio<DTO.FlujoPaso> servicio) =>
                await servicio.ListarPorId(id)
                    is DTO.FlujoPaso flujo_paso
                        ? TypedResults.Ok<DTO.FlujoPaso>(flujo_paso)
                        : TypedResults.NotFound());

            flujo_pasos.MapGet("/flow/{flowId}", static async (int flowId, IServicio<DTO.FlujoPaso> servicio) =>
                await ((Services.FlujoPaso)servicio).ListarPorFlujoId(flowId));

            flujo_pasos.MapPost("/", static async Task<Results<Created<DTO.FlujoPaso>, BadRequest<Dominio.Error>>> (
                DTO.FlujoPaso flujo_paso, IServicio<DTO.FlujoPaso> servicio) =>
            {
                // BR: Cada paso dentro de un flujo, tiene un autorizador pre-asignado
                if (!await ((Services.FlujoPaso)servicio).EsUsuarioValido(flujo_paso.UsuarioAsignadoId))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"El usuario ID: {flujo_paso.UsuarioAsignadoId}, ¡no es válido!",
                        "Cada paso dentro del flujo, debe tener un autorizador pre-asignado."
                    ));
                // BR: Cada paso tiene un orden especifico
                // BR: Todos los flujos son secuenciales
                if (!await ((Services.FlujoPaso)servicio).EsSecuenciaValida(flujo_paso.Orden, flujo_paso.FlujoId))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"La secuencia del paso, ID: {flujo_paso.Orden}, ¡no es válida!",
                        "Cada paso del flujo, debe tener un orden específico y/o secuencial."
                    ));
                try {
                    flujo_paso = await servicio.Insertar(flujo_paso);
                    return TypedResults.Created($"/api/apprflow/flow-step/{flujo_paso.Id}", flujo_paso);
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
            flujo_pasos.MapPut("/{id}", static async Task<Results<NoContent, NotFound, BadRequest<Dominio.Error>>> (
                int id, DTO.FlujoPaso flujo_paso, IServicio<DTO.FlujoPaso> servicio) =>
            {
                return await servicio.Reemplazar(id, flujo_paso)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
            //  To support partial updates, use HTTP PATCH.
            flujo_pasos.MapPatch("/{flujo_id}/{paso_orden}", static async Task<Results<NoContent, NotFound, BadRequest<Dominio.Error>>> (
                int flujo_id, int paso_orden, DTO.FlujoPaso flujo_paso, IServicio<DTO.FlujoPaso> servicio) =>
            {
                // BR: Para tomar una decisión APROBADO o RECHAZADO, se manda una observación
                if (!((Services.FlujoPaso)servicio).EsDesicionValida(flujo_paso.Estado, flujo_paso.Observacion ?? ""))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"El estado '{flujo_paso.Estado}' y/o la observación, ¡no son válidos!",
                        "Para tomar una decisión APROBADO o RECHAZADO, se debe mandar una observación."
                    ));
                // BR: Solamente el autorizador o el administrador pueden APROBAR/RECHAZAR un paso
                if (!await ((Services.FlujoPaso)servicio).EsAutorizadorValido(flujo_id, paso_orden, flujo_paso.Estado, flujo_paso.UsuarioDecisionId ?? 0))
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.BR),
                        $"El usuario ID: {flujo_paso.UsuarioDecisionId} ¡no es válido!",
                        "Solamente el autorizador o un administrador pueden APROBAR/RECHAZAR el paso."
                    ));
                try {
                    return await servicio.Actualizar(0, flujo_paso)
                        ? TypedResults.NoContent()
                        : TypedResults.NotFound();
                } catch (Exception ex) {
                    return TypedResults.BadRequest(error: new Dominio.Error(
                        Dominio.Codigo(Dominio.TipoOp.UPD),
                        $"{ex.Message}",
                        $"{ex.InnerException?.Message}"
                    ));
                }
            });

            flujo_pasos.MapDelete("/{id}", static async Task<Results<NoContent, NotFound>> (int id, IServicio<DTO.FlujoPaso> servicio) =>
            {
                if (await servicio.Eliminar(id))
                    return TypedResults.NoContent();
                return TypedResults.NotFound();
            });
        }
    }
}
