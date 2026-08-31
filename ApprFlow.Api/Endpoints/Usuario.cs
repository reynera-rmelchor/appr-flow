using ApprFlow.Api.Services.Core;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ApprFlow.Api.Endpoints
{
    public static class Usuario
    {
        internal static void MapUsuarios(this IEndpointRouteBuilder app)
        {
            var usuarios = app.MapGroup("/api/apprflow/user");

            usuarios.MapGet("/", static async (IServicio<DTO.Usuario> servicio) =>
                await servicio.Listar());

            usuarios.MapGet("/{id}", static async Task<Results<Ok<DTO.Usuario>, NotFound>> (int id, IServicio<DTO.Usuario> servicio) =>
                await servicio.ListarPorId(id)
                    is DTO.Usuario usuario
                        ? TypedResults.Ok<DTO.Usuario>(usuario)
                        : TypedResults.NotFound());

            usuarios.MapGet("/role/{rolId}", static async (int rolId, IServicio<DTO.Usuario> servicio) =>
                await ((Services.Usuario)servicio).ListarPorRol(rolId));
            
            usuarios.MapPost("/", static async Task<Results<Created<DTO.Usuario>, BadRequest<Dominio.Error>>>  (
                DTO.Usuario usuario, IServicio<DTO.Usuario> servicio) =>
            {   
                try {
                    usuario = await servicio.Insertar(usuario);
                    return TypedResults.Created($"/api/apprflow/user/{usuario.Id}", usuario);
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
            usuarios.MapPut("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Usuario usuario, IServicio<DTO.Usuario> servicio) =>
            {
                return await servicio.Reemplazar(id, usuario)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
            //  To support partial updates, use HTTP PATCH.
            usuarios.MapPatch("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, DTO.Usuario usuario, IServicio<DTO.Usuario> servicio) =>
            {
                return await servicio.Actualizar(id, usuario)
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });

            usuarios.MapDelete("/{id}", static async Task<Results<NoContent, NotFound>> (
                int id, IServicio<DTO.Usuario> servicio) =>
            {
                return await servicio.Eliminar(id) 
                    ? TypedResults.NoContent()
                    : TypedResults.NotFound();
            });
        }
    }
}
