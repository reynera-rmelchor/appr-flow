namespace ApprFlow.Api.Services.Core
{
    public interface IServicio<T> where T : class
    {
        Task<IEnumerable<T>> Listar();
        Task<T> ListarPorId(int id);
        Task<T> Insertar(T dto);
        Task<bool> Reemplazar(int id, T dto);
        Task<bool> Actualizar(int id, T dto);
        Task<bool> Eliminar(int id);
    }
}
