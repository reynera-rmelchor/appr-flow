using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Services.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace ApprFlow.Api.Services
{
    public class PlantillaPaso : IServicio<DTO.PlantillaPaso>
    {
        private readonly ContextoBD _context;
        private readonly IMapper _mapper;

        public PlantillaPaso(ContextoBD db, IMapper mapper)
        {
            this._context = db;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<DTO.PlantillaPaso>> Listar()
        {
            var plantilla_pasos = await _context.PlantillaPasos.ToListAsync();

            return plantilla_pasos.Select(t => _mapper.Map<DTO.PlantillaPaso>(t));
        }
        public async Task<DTO.PlantillaPaso> ListarPorId(int id)
        {
            var plantilla_paso = await _context.PlantillaPasos.FindAsync(id);

            return _mapper.Map<DTO.PlantillaPaso>(plantilla_paso);
        }
        public async Task<IEnumerable<DTO.PlantillaPaso>> ListarPorPlantillaId(int id)
        {
            var plantilla_pasos = await _context.PlantillaPasos.Where(t => t.PlantillaId == id).ToListAsync();

            return plantilla_pasos.Select(t => _mapper.Map<DTO.PlantillaPaso>(t));
        }
        public async Task<DTO.PlantillaPaso> Insertar(DTO.PlantillaPaso dto)
        {
            try {
                var plantilla_paso = _mapper.Map<Models.PlantillaPaso>(dto);

                _context.PlantillaPasos.Add(plantilla_paso);
                await _context.SaveChangesAsync();
                return _mapper.Map<DTO.PlantillaPaso>(plantilla_paso);
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
        public async Task<bool> Reemplazar(int id, DTO.PlantillaPaso dto)
        {
            var plantilla_paso = await _context.PlantillaPasos.FindAsync(id);
            if (plantilla_paso is null) return false;

            plantilla_paso.PlantillaId = dto.PlantillaId;
            plantilla_paso.Orden = dto.Orden;
            plantilla_paso.UsuarioAprobadorId = dto.UsuarioAprobadorId;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Actualizar(int id, DTO.PlantillaPaso dto)
        {
            var plantilla_paso = await _context.PlantillaPasos.SingleAsync(p => p.PlantillaId == dto.PlantillaId && p.Orden == dto.Orden);
            if (plantilla_paso is null) return false;

            plantilla_paso.UsuarioAprobadorId = dto.UsuarioAprobadorId;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> Eliminar(int id)
        {
            var plantilla_paso = await _context.PlantillaPasos.FindAsync(id);
            if (plantilla_paso is null) return false;

            _context.PlantillaPasos.Remove(plantilla_paso);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> EsUsuarioValido(int usuarioId)
        {
            return usuarioId >= Dominio.MIN_VAL &&
                await _context.Usuarios
                    .Where(u => u.Id == usuarioId && u.Activo)
                    .FirstOrDefaultAsync() is not null;
        }
        public async Task<bool> EsSecuenciaValida(int orden, int plantillaId)
        {
            // - El 'Orden' sea mayor o igual al valor mínimo definido
            // - No exista otro paso con el mismo 'Orden'
            // - El 'Orden' sea mayor al del último paso de la plantilla (si existe) 
            return orden >= Dominio.MIN_VAL &&
                !await _context.PlantillaPasos.AnyAsync(p => p.PlantillaId == plantillaId && p.Orden == orden) &&
                orden > (await _context.PlantillaPasos.Where(p => p.PlantillaId == plantillaId).MaxAsync(p => (int?)p.Orden) ?? 0);
        }
    }
}
