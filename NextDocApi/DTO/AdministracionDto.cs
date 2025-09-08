namespace NextDocApi.DTO
{
    public class AdministracionDto
    {
    }
    public class DepartamentoDto
    {
        public int IdDepartamento { get; set; }

        public string NombreDepartamento { get; set; } = null!;

        public bool? Estado { get; set; }
    }

    public class RolDto
    {
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class ClasificacionDto
    {
        public int IdClasificacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class EstadoDocumentoDto
    {
        public int IdEstado { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class PermisoAccesoDto
    {
        public int IdPermiso { get; set; }
        public int? IdUsuario { get; set; }
        public string TagPantallaId { get; set; } = string.Empty;
        public bool? PuedeVer { get; set; }
        public bool? PuedeEditar { get; set; }
    }

    public class TipoDocumentoDto
    {
        public int IdTipoDocumento { get; set; }
        public string NombreTipo { get; set; } = string.Empty;
        public bool? Estado { get; set; }
    }

    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NroWhatsapp { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public int? IdDepartamento { get; set; }
        public bool? Estado { get; set; }
        public string? Password { get; set; }
    }
}
