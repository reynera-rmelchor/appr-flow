namespace ApprFlow.Api.DTO;

public partial class Flujo
{
    public int Id { get; set; }

    public int PlantillaId { get; set; }

    public int UsuarioCreadorId { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public byte Estado { get; set; }

    public DateTime CreadoEn { get; set; }

    public DateTime? ActualizadoEn { get; set; }

    public ICollection<FlujoPaso> FlujoPasos { get; set; } = new List<FlujoPaso>();
}
