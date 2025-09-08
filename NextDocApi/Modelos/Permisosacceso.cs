using System;
using System.Collections.Generic;

namespace NextDocApi.Modelos;

public partial class PermisosAcceso
{
    public int IdPermiso { get; set; }

    public string? TagPantallaId { get; set; }

    public int? IdUsuario { get; set; }

    public bool? PuedeVer { get; set; }

    public bool? PuedeEditar { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
