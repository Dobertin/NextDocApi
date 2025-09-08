using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class EstadosDocumento
{
    public int IdEstado { get; set; }

    public string NombreEstado { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
