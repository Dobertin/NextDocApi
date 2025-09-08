using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public int? IdDepartamento { get; set; }

    public bool? Estado { get; set; }

    public string? NroWhatsapp { get; set; }

    public virtual ICollection<Documento> DocumentoIdUsuarioAsignadoNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<Documento> DocumentoIdUsuarioCreadorNavigations { get; set; } = new List<Documento>();

    public virtual ICollection<HistorialDocumento> HistorialDocumentos { get; set; } = new List<HistorialDocumento>();

    public virtual Departamento? IdDepartamentoNavigation { get; set; }

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual ICollection<PermisosAcceso> PermisosAccesos { get; set; } = new List<PermisosAcceso>();
}
