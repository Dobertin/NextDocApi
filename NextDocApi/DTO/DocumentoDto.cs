using System.ComponentModel.DataAnnotations;

namespace NextDocApi.DTO
{
    public class DocumentoDto
    {
        public int IdDocumento { get; set; }

        public string Titulo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string RutaArchivo { get; set; } = null!;

        public int? IdTipoDocumento { get; set; }

        public string NombreTipoDocumento { get; set; } = null!;

        public int? IdClasificacion { get; set; }

        public string NombreClasificacion { get; set; } = null!;

        public int? IdEstado { get; set; }

        public string NombreEstadoDocumento { get; set; } = null!;

        public int? IdUsuarioCreador { get; set; }

        public int? IdUsuarioAsignado { get; set; }

        public string NombreUsuarioAsignado { get; set; } = null!;

        public int? IdDepartamento { get; set; }

        public string NombreDepartamento { get; set; } = null!;

        public bool? Estado { get; set; }

        public DateTime? Fecha { get; set; }
    }
    public class DocumentoRegistroDto
    {
        public IFormFile? Archivo { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        public int? IdTipoDocumento { get; set; }

        public int? IdClasificacion { get; set; }

        public int? IdEstado { get; set; }

        public int? IdUsuarioCreador { get; set; }

        public int? IdUsuarioAsignado { get; set; }

        public int? IdDepartamento { get; set; }

        public string? Comentarios { get; set; } // opcional para historial

        public int? IdDocumentoRelacionado { get; set; }
    }
    public class DocumentoChangeUserDto
    {
        public int IdDocumento { get; set; }
        public int IdUsuarioAsignado { get; set; }
        public int IdUsuarioModifica { get;set; }
    }
    public class DocumentoChangeStateDto
    {
        public int IdDocumento { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuarioModifica { get; set; }
    }
    public class DocumentoFiltroDto
    {
        public int IdEstado { get; set; }
        public int IdDepartamento { get; set; }
        public int IdClasificacion { get; set; }
        public string? TextoBusqueda { get; set; }  // Nuevo
        public int PageNumber { get; set; } = 1;     // Nuevo
        public int PageSize { get; set; } = 10;      // Nuevo
    }
    public class ReporteDocumentoFiltroDto
    {
        public int IdEstado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int IdClasificacion { get; set; }
        public int PageNumber { get; set; } = 1;     // Nuevo
        public int PageSize { get; set; } = 10;      // Nuevo
    }
    public class UpdateArchivoDto
    {
        [Required]
        public IFormFile Archivo { get; set; }

        [Required]
        public int IdUsuario { get; set; } // si vas a registrar historial

        public string? Comentarios { get; set; }
    }
    public class HistorialDocumentoDto
    {
        public DateTime? FechaAccion { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public string Responsable { get; set;} = string.Empty;
    }

}
