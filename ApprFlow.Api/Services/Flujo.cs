using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Services.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Services
{
    public class Flujo : IServicio<DTO.Flujo>
    {
        private readonly ContextoBD _context;
        private readonly IMapper _mapper;

        public Flujo(ContextoBD db, IMapper mapper)
        {
            this._context = db;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<DTO.Flujo>> Listar()
        {
            var flujos = await _context.Flujos.ToListAsync();

            return flujos.Select(f => _mapper.Map<DTO.Flujo>(f));
        }
        public async Task<DTO.Flujo> ListarPorId(int id)
        {
            var flujo = await _context.Flujos
                .Include(f => f.FlujoPasos)
                .FirstOrDefaultAsync(f => f.Id == id);
            return _mapper.Map<DTO.Flujo>(flujo);
        }
        public async Task<IEnumerable<DTO.Flujo>> ListarPorEstado(int status)
        {
            var flujos = await _context.Flujos.Where(f => f.Estado == status).ToListAsync();

            return flujos.Select(f => _mapper.Map<DTO.Flujo>(f));
        }
        public async Task<DTO.Flujo> Insertar(DTO.Flujo dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                // Insertar flujo principal: obtener nuevo ID
                var flujo = _mapper.Map<Models.Flujo>(dto);
                _context.Flujos.Add(flujo);
                await _context.SaveChangesAsync();

                // Obtener pasos de la plantilla
                var pasos = await _context.PlantillaPasos.Where(pp => pp.PlantillaId == dto.PlantillaId).ToListAsync();
                // Por cada paso de la plantilla, crear paso en flujo
                foreach (var p in pasos) {
                    var flujo_paso = new Models.FlujoPaso {
                        FlujoId = flujo.Id,
                        Orden = p.Orden,
                        UsuarioAsignadoId = p.UsuarioAprobadorId
                    };
                    _context.FlujoPasos.Add(flujo_paso);
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                //Obtenemos los pasos del flujo creado, para devolverlos en la respuesta
                flujo.FlujoPasos = await _context.FlujoPasos.Where(fp => fp.FlujoId == flujo.Id).ToListAsync();
                return _mapper.Map<DTO.Flujo>(flujo);
            } catch (DbUpdateException db_ex) {
                await transaction.RollbackAsync();
                // Extrae mensaje detallado
                var inner_msg = db_ex.InnerException?.Message ?? db_ex.Message;
                // Lanza excepción con el contexto claro de BD
                throw new Exception(Dominio.Mensaje(Dominio.TipoEx.BD), new Exception($"{inner_msg}", db_ex));
            } catch (Exception ex) {
                await transaction.RollbackAsync();
                // Captura errores lógicos, mapeo o nulos, antes de llegar a BD
                throw new Exception(Dominio.Mensaje(Dominio.TipoEx.GRAL), new Exception($"{ex.Message}", ex));
            }
        }
        public async Task<bool> Reemplazar(int id, DTO.Flujo dto)
        {
            var flujo = await _context.Flujos.FindAsync(id);
            if (flujo is null) return false;

            flujo.PlantillaId = dto.PlantillaId;
            flujo.UsuarioCreadorId = dto.UsuarioCreadorId;
            flujo.Titulo = dto.Titulo;
            flujo.Descripcion = dto.Descripcion;
            flujo.Estado = dto.Estado;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Actualizar(int id, DTO.Flujo dto)
        {
            var flujo = await _context.Flujos.FindAsync(id);
            if (flujo is null) return false;

            if (dto.Titulo is not null) flujo.Titulo = dto.Titulo;
            if (dto.Descripcion is not null) flujo.Descripcion = dto.Descripcion;
            flujo.PlantillaId = dto.PlantillaId;
            flujo.UsuarioCreadorId = dto.UsuarioCreadorId;
            flujo.Estado = dto.Estado;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var flujo = await _context.Flujos.FindAsync(id);
            if (flujo is null) return false;

            _context.Flujos.Remove(flujo);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> EsPlantillaValida(int plantillaId)
        {
            return await _context.Plantillas
                .Where(p => p.Id == plantillaId && p.Activo)
                .FirstOrDefaultAsync() is not null;
        }
    }
}
