namespace ApprFlow.Api.DTO;

public partial class Plantilla
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<PlantillaPaso> PlantillaPasos { get; set; } = new List<PlantillaPaso>();
}
