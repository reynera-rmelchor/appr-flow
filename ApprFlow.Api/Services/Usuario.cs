using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Services.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Services
{
    public class Usuario : IServicio<DTO.Usuario>
    {
        private readonly ContextoBD _context;
        private readonly IMapper _mapper;

        public Usuario(ContextoBD db, IMapper mapper)
        {
            this._context = db;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<DTO.Usuario>> Listar()
        {
            var usuarios = await _context.Usuarios.ToListAsync();

            return usuarios.Select(u => _mapper.Map<DTO.Usuario>(u));
        }
        public async Task<DTO.Usuario> ListarPorId(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            return _mapper.Map<DTO.Usuario>(usuario);
        }
        public async Task<IEnumerable<DTO.Usuario>> ListarPorRol(int rolId)
        {
            var usuarios = await _context.Usuarios.Where(u => u.Rol == rolId).ToListAsync();

            return usuarios.Select(u => _mapper.Map<DTO.Usuario>(u));
        }
        public async Task<DTO.Usuario> Insertar(DTO.Usuario dto)
        {
            try {
                var usuario = _mapper.Map<Models.Usuario>(dto);

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
                return _mapper.Map<DTO.Usuario>(usuario);
            } catch (DbUpdateException db_ex) {
                // Extrae mensaje detallado
                var inner_msg = db_ex.InnerException?.Message ?? db_ex.Message;
                // Lanza excepción con el contexto claro de BD
                throw new Exception(Dominio.Mensaje(Dominio.TipoEx.BD), new Exception($"{inner_msg}", db_ex));
            } catch (Exception ex) {
                // Captura errores lógicos, mapeo o nulos, antes de llegar a BD
                throw new Exception(Dominio.Mensaje(Dominio.TipoEx.GRAL), new Exception($"{ex.Message}", ex));
            }
        }
        public async Task<bool> Reemplazar(int id, DTO.Usuario dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return false;

            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            usuario.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Actualizar(int id, DTO.Usuario dto)
        {
            var usuario= await _context.Usuarios.FindAsync(id);
            if (usuario is null) return false;

            if (dto.Nombre is not null) usuario.Nombre = dto.Nombre;
            if (dto.Email is not null) usuario.Email = dto.Email;
            usuario.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario is null) return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
