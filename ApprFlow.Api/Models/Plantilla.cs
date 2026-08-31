using System;
using System.Collections.Generic;

namespace ApprFlow.Api.Models;

public partial class Plantilla
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual ICollection<Flujo> Flujos { get; set; } = new List<Flujo>();

    public virtual ICollection<PlantillaPaso> PlantillaPasos { get; set; } = new List<PlantillaPaso>();
}
