using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class Clasificacion
{
    public int IdClasificacion { get; set; }

    public string Nombre { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
