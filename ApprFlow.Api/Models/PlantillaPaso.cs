using System;
using System.Collections.Generic;

namespace ApprFlow.Api.Models;

public partial class PlantillaPaso
{
    public int Id { get; set; }

    public int PlantillaId { get; set; }

    public int Orden { get; set; }

    public int UsuarioAprobadorId { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Plantilla Plantilla { get; set; } = null!;

    public virtual Usuario UsuarioAprobador { get; set; } = null!;
}
