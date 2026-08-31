using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Services.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Services
{
    public class Plantilla : IServicio<DTO.Plantilla>
    {
        private readonly ContextoBD _context;
        private readonly IMapper _mapper;

        public Plantilla(ContextoBD db, IMapper mapper)
        {
            this._context = db;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<DTO.Plantilla>> Listar()
        {
            var plantillas = await _context.Plantillas.ToListAsync();

            return plantillas.Select(p => _mapper.Map<DTO.Plantilla>(p));
        }
        public async Task<DTO.Plantilla> ListarPorId(int id)
        {
            var plantilla = await _context.Plantillas.FindAsync(id);

            return _mapper.Map<DTO.Plantilla>(plantilla);
        }
        public async Task<IEnumerable<DTO.Plantilla>> ListarPorEstado(bool status)
        {
            var plantillas = await _context.Plantillas.Where(p => p.Activo == status).ToListAsync();

            return plantillas.Select(p => _mapper.Map<DTO.Plantilla>(p));
        }
        public async Task<DTO.Plantilla> Insertar(DTO.Plantilla dto)
        {
            try {
                var plantilla = _mapper.Map<Models.Plantilla>(dto);

                _context.Plantillas.Add(entity: plantilla);
                await _context.SaveChangesAsync();
                return _mapper.Map<DTO.Plantilla>(plantilla);
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
        public async Task<bool> Reemplazar(int id, DTO.Plantilla dto)
        {
            var plantilla = await _context.Plantillas.FindAsync(id);
            if (plantilla is null) return false;

            plantilla.Nombre = dto.Nombre;
            plantilla.Descripcion = dto.Descripcion;
            plantilla.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Actualizar(int id, DTO.Plantilla dto)
        {
            var plantilla = await _context.Plantillas.FindAsync(id);
            if (plantilla is null) return false;

            if (dto.Nombre is not null) plantilla.Nombre = dto.Nombre;
            if (dto.Descripcion is not null) plantilla.Descripcion = dto.Descripcion;
            plantilla.Activo = dto.Activo;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var plantilla = await _context.Plantillas.FindAsync(id);
            if (plantilla is null) return false;

            _context.Plantillas.Remove(plantilla);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
