using System;
using System.Collections.Generic;

namespace ApprFlow.Api.Models;

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

    public virtual ICollection<FlujoPaso> FlujoPasos { get; set; } = new List<FlujoPaso>();

    public virtual Plantilla Plantilla { get; set; } = null!;

    public virtual Usuario UsuarioCreador { get; set; } = null!;
}
