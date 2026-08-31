namespace ApprFlow.Api.DTO;

public partial class FlujoPaso
{
    public int Id { get; set; }

    public int FlujoId { get; set; }

    public int Orden { get; set; }

    public int UsuarioAsignadoId { get; set; }

    public int? UsuarioDecisionId { get; set; }

    public byte Estado { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaDecision { get; set; }
}
