using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class Departamento
{
    public int IdDepartamento { get; set; }

    public string NombreDepartamento { get; set; } = null!;

    public bool? Estado { get; set; }

    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
