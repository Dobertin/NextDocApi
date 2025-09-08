using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class HistorialDocumento
{
    public int IdHistorial { get; set; }

    public int IdDocumento { get; set; }

    public int IdUsuario { get; set; }

    public string? Accion { get; set; }

    public DateTime? FechaAccion { get; set; }

    public string? Comentarios { get; set; }

    public bool? Estado { get; set; }

    public virtual Documento IdDocumentoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
