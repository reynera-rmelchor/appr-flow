namespace ApprFlow.Api.DTO;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public byte Rol { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }
}
