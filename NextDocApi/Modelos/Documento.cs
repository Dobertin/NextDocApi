using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class Documento
{
    public int IdDocumento { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string RutaArchivo { get; set; } = null!;

    public DateTime? FechaCreacion { get; set; }

    public int? IdTipoDocumento { get; set; }

    public int? IdClasificacion { get; set; }

    public int? IdEstado { get; set; }

    public int? IdUsuarioCreador { get; set; }

    public int? IdUsuarioAsignado { get; set; }

    public int? IdDepartamento { get; set; }

    public bool? Estado { get; set; }

    public int? IdDocumentoRelacionado { get; set; }

    public virtual ICollection<HistorialDocumento> HistorialDocumentos { get; set; } = new List<HistorialDocumento>();

    public virtual Clasificacion? IdClasificacionNavigation { get; set; }

    public virtual Departamento? IdDepartamentoNavigation { get; set; }

    public virtual EstadosDocumento? IdEstadoNavigation { get; set; }

    public virtual TiposDocumento? IdTipoDocumentoNavigation { get; set; }

    public virtual Usuario? IdUsuarioAsignadoNavigation { get; set; }

    public virtual Usuario? IdUsuarioCreadorNavigation { get; set; }
}
