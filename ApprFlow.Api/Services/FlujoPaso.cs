using ApprFlow.Api.Models.Context;
using ApprFlow.Api.Services.Core;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ApprFlow.Api.Services
{
    public class FlujoPaso : IServicio<DTO.FlujoPaso>
    {
        private readonly ContextoBD _context;
        private readonly IMapper _mapper;

        public FlujoPaso(ContextoBD db, IMapper mapper)
        {
            this._context = db;
            this._mapper = mapper;
        }

        public async Task<IEnumerable<DTO.FlujoPaso>> Listar()
        {
            var flujo_pasos = await _context.FlujoPasos.ToListAsync();

            return flujo_pasos.Select(t => _mapper.Map<DTO.FlujoPaso>(t));
        }
        public async Task<DTO.FlujoPaso> ListarPorId(int id)
        {
            var flujo_paso = await _context.FlujoPasos.FindAsync(id);

            return _mapper.Map<DTO.FlujoPaso>(flujo_paso);
        }
        public async Task<IEnumerable<DTO.FlujoPaso>> ListarPorFlujoId(int id)
        {
            var flujo_pasos = await _context.FlujoPasos.Where(t => t.FlujoId == id).ToListAsync();

            return flujo_pasos.Select(t => _mapper.Map<DTO.FlujoPaso>(t));
        }
        public async Task<DTO.FlujoPaso> Insertar(DTO.FlujoPaso dto)
        {
            try {
                var flujo_paso = _mapper.Map<Models.FlujoPaso>(dto);

                _context.FlujoPasos.Add(flujo_paso);
                await _context.SaveChangesAsync();
                return _mapper.Map<DTO.FlujoPaso>(flujo_paso);
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
        public async Task<bool> Reemplazar(int id, DTO.FlujoPaso dto)
        {
            try {
                var flujo_paso = await _context.FlujoPasos.FindAsync(id);
                if (flujo_paso is null) return false;

                flujo_paso.FlujoId = dto.FlujoId;
                flujo_paso.Orden = dto.Orden;
                flujo_paso.UsuarioAsignadoId = dto.UsuarioAsignadoId;
                await _context.SaveChangesAsync();
                return true;
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
        public async Task<bool> Actualizar(int id, DTO.FlujoPaso dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                var flujo_paso = await _context.FlujoPasos.SingleAsync(p => p.FlujoId == dto.FlujoId && p.Orden == dto.Orden);
                if (flujo_paso is null) return false;

                flujo_paso.Estado = dto.Estado;
                flujo_paso.UsuarioDecisionId = dto.UsuarioDecisionId;
                flujo_paso.FechaDecision = DateTime.Now;
                flujo_paso.Observacion = dto.Observacion;
                await _context.SaveChangesAsync();
                // BR: Si se rechaza un paso...
                if (flujo_paso.Estado == (byte)Dominio.TipoEdo.REJECTED)
                {
                    // BR: ... se rechaza el flujo
                    await _context.Flujos
                        .Where(f => f.Id == flujo_paso.FlujoId)
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(f => f.Estado, (byte)Dominio.TipoEdo.REJECTED)
                            .SetProperty(f => f.ActualizadoEn, DateTime.Now)
                        );
                    //BR: ... y se rechazan todos los pasos pendientes del flujo
                    await _context.FlujoPasos
                        .Where(p => p.FlujoId == dto.FlujoId && p.Orden > dto.Orden && p.Estado == (byte)Dominio.TipoEdo.PENDING)
                        .ExecuteUpdateAsync(r => r
                            .SetProperty(p => p.Estado, (byte)Dominio.TipoEdo.REJECTED)
                            .SetProperty(p => p.UsuarioDecisionId, dto.UsuarioDecisionId)
                            .SetProperty(p => p.FechaDecision, DateTime.Now)
                            .SetProperty(p => p.Observacion, "Estado BLOQUEADO por BR")
                        );
                }
                await transaction.CommitAsync();
                return true;
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
        public async Task<bool> Eliminar(int id)
        {
            var flujo_paso = await _context.FlujoPasos.FindAsync(id);
            if (flujo_paso is null) return false;

            _context.FlujoPasos.Remove(flujo_paso);
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
        public async Task<bool> EsSecuenciaValida(int orden, int flujoId)
        {
            // - El 'Orden' sea mayor o igual al valor mínimo definido
            // - No exista otro paso con el mismo 'Orden'
            // - El 'Orden' sea mayor al del último paso de la plantilla (si existe) 
            return orden >= Dominio.MIN_VAL &&
                !await _context.FlujoPasos.AnyAsync(p => p.Orden == orden) &&
                orden > (await _context.FlujoPasos.Where(p => p.FlujoId == flujoId).MaxAsync(p => (int?)p.Orden) ?? 0);
        }
        public bool EsDesicionValida(int estado, string observacion)
        {
            return (estado == (int)Dominio.TipoEdo.APPROVED || 
                estado == (int)Dominio.TipoEdo.REJECTED) &&
                !string.IsNullOrWhiteSpace(observacion);
        }
        public async Task<bool> EsAutorizadorValido(int flujoId,int orden, int estado, int usuarioId)
        {
            // BR: El usuario que toma la decisión, sea el mismo que el asignado al paso o un administrador
            return await this.EsUsuarioValido(usuarioId) &&
                // Si es una APROBACIÓN, el usuario puede ser un administrador o el asignado al paso
                (estado != (int)Dominio.TipoEdo.APPROVED || 
                    await _context.Usuarios
                        .AnyAsync(u => u.Id == usuarioId && u.Rol == (int)Dominio.TipoRol.ADM) ||
                    await _context.FlujoPasos
                        .AnyAsync(
                               p => p.FlujoId == flujoId
                            && p.Orden == orden
                            && p.Estado == (int)Dominio.TipoEdo.PENDING
                            && p.UsuarioAsignadoId == usuarioId)
                ) &&
                // Si es un RECHAZO, el usuario debe ser el asignado al paso
                await _context.FlujoPasos
                    .AnyAsync(
                           p => p.FlujoId == flujoId
                        && p.Orden == orden
                        && p.Estado == (int)Dominio.TipoEdo.PENDING
                        && p.UsuarioAsignadoId == usuarioId);
        }
    }
}
