using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class TiposDocumento
{
    public int IdTipoDocumento { get; set; }

    public string NombreTipo { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
