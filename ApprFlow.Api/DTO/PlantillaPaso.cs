namespace ApprFlow.Api.DTO;

public partial class PlantillaPaso
{
    public int Id { get; set; }

    public int PlantillaId { get; set; }

    public int Orden { get; set; }

    public int UsuarioAprobadorId { get; set; }

    public DateTime CreadoEn { get; set; }
}
