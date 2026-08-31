using System;
using System.Collections.Generic;

namespace ApprFlow.Api.Models;

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

    public virtual Flujo Flujo { get; set; } = null!;

    public virtual Usuario UsuarioAsignado { get; set; } = null!;

    public virtual Usuario? UsuarioDecision { get; set; }
}
