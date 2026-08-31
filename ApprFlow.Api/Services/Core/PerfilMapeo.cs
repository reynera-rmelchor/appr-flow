using AutoMapper;

namespace ApprFlow.Api.Services.Core
{
    public class PerfilMapeo : Profile
    {
        public PerfilMapeo()
        {
            // Crear regla de mapeo direccional de Usuario -> UsuarioDTO a directional mapping rule from User -> UserDto
            CreateMap<Models.Usuario, DTO.Usuario>();
            CreateMap<Models.Plantilla, DTO.Plantilla>();
            CreateMap<Models.PlantillaPaso, DTO.PlantillaPaso>();
            CreateMap<Models.Flujo, DTO.Flujo>();
            CreateMap<Models.FlujoPaso, DTO.FlujoPaso>();

            // Mapeo inverso (UsuarioDto -> Usuario)
            CreateMap<DTO.Usuario, Models.Usuario>();
            CreateMap<DTO.Plantilla, Models.Plantilla>();
            CreateMap<DTO.PlantillaPaso, Models.PlantillaPaso>();
            CreateMap<DTO.Flujo, Models.Flujo>();
            CreateMap<DTO.FlujoPaso, Models.FlujoPaso>();
        }
    }

}
