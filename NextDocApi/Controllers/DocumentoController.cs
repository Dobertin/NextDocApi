using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;
using NextDocApi.Helper;
using NextDocApi.Modelos;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Xml.XPath;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public DocumentoController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private static Expression<Func<Documento, DocumentoDto>> DocumentoSelector => d => new DocumentoDto
        {
            IdDocumento = d.IdDocumento,
            Titulo = d.Titulo,
            Descripcion = d.Descripcion,
            IdDepartamento = d.IdDepartamento,
            IdTipoDocumento = d.IdTipoDocumento,
            IdClasificacion = d.IdClasificacion,
            Estado = d.Estado,
            IdEstado = d.IdEstado,
            IdUsuarioAsignado = d.IdUsuarioAsignado,
            IdUsuarioCreador = d.IdUsuarioCreador,
            RutaArchivo = d.RutaArchivo,
            Fecha = d.FechaCreacion,
            // Nuevas propiedades relacionadas
            NombreClasificacion = d.IdClasificacionNavigation.Nombre,
            NombreTipoDocumento = d.IdTipoDocumentoNavigation.NombreTipo,
            NombreEstadoDocumento = d.IdEstadoNavigation.NombreEstado,
            NombreDepartamento = d.IdDepartamentoNavigation.NombreDepartamento,
            NombreUsuarioAsignado = $"{d.IdUsuarioAsignadoNavigation.Nombres} {d.IdUsuarioAsignadoNavigation.Apellidos}"
        };

        private (int IdUsuario, int IdRol) ObtenerUsuarioYRolDesdeToken()
        {
            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var idRol = int.Parse(User.FindFirst("IdRol")?.Value ?? "0"); // <-- usa el nuevo claim "IdRol"
            return (idUsuario, idRol);
        }

        private async Task RegistrarHistorialDocumentoAsync(int idDocumento, int idUsuario, string accion, string? comentarios = null)
        {
            var historial = new HistorialDocumento
            {
                IdDocumento = idDocumento,
                IdUsuario = idUsuario,
                Accion = accion,
                Comentarios = comentarios,
                FechaAccion = DateTime.UtcNow.AddHours(-5),
                Estado = true
            };

            _context.HistorialDocumentos.Add(historial);
            await _context.SaveChangesAsync();
        }

        [Authorize]
        [HttpDelete("{idDocumento}")]
        public async Task<IActionResult> Delete(int idDocumento)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var documento = await _context.Documentos.FindAsync(idDocumento);

                if (documento is null)
                {
                    return NotFound(new RespuestaDto<string>(false, "Documento no encontrado.", null));
                }

                _context.Documentos.Remove(documento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new RespuestaDto<string>(true, "Documento eliminado correctamente.", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpPost("ChangeEstateDocument")]
        public async Task<IActionResult> ChangeEstateDocument([FromBody] DocumentoChangeStateDto data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var doc = new Documento { IdDocumento = data.IdDocumento };
                _context.Documentos.Attach(doc);
                doc.IdEstado = data.IdEstado;
                doc.FechaCreacion = DateTime.UtcNow.AddHours(-5);
                var state = await _context.EstadosDocumentos.FirstOrDefaultAsync(x=> x.IdEstado==data.IdEstado);

                _context.Entry(doc).Property(x => x.IdEstado).IsModified = true;
                _context.Entry(doc).Property(x => x.FechaCreacion).IsModified = true;
                if (state.IdEstado == (int)EstadoDocumento.Enviado)
                {
                    var user = await _context.Usuarios.FirstOrDefaultAsync(z => z.IdRol == 3);
                    if (user != null) {
                        doc.IdUsuarioAsignado = user.IdUsuario;
                        _context.Entry(doc).Property(x => x.IdUsuarioAsignado).IsModified = true;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();
                await RegistrarHistorialDocumentoAsync(
                    idDocumento: data.IdDocumento,
                    idUsuario: idUsuarioToken,
                    accion: $"Cambio a estado {state.NombreEstado}",
                    comentarios: $"Cambio hecho por el Usuario con Id {data.IdUsuarioModifica}."
                );

                return Ok(new RespuestaDto<string>(true, $"Documento {data.IdDocumento} actualizado correctamente.", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpPost("UpdateDataDocument")]
        public async Task<IActionResult> UpdateDataDocument([FromBody] DocumentoDto documento)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new RespuestaDto<List<string>>(false, "Errores de validación.", errores));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = await _context.Documentos.FindAsync(documento.IdDocumento);
                if (doc == null)
                    return NotFound(new RespuestaDto<string>(false, "Documento no encontrado.", null));

                // Actualización
                _context.Entry(doc).CurrentValues.SetValues(documento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new RespuestaDto<string>(true, "Actualización exitosa", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpPost("UpdateFileDocument/{idDocumento}")]
        public async Task<IActionResult> UpdateFileDocument(int idDocumento, [FromForm] UpdateArchivoDto archivo)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            if (archivo == null || archivo.Archivo.Length == 0)
                return BadRequest(new RespuestaDto<string>(false, "Debe enviar un archivo.", null));

            // Ruta base donde se guardan los archivos
            var carpetaDocumentos = Path.Combine(Directory.GetCurrentDirectory(), "Archivos", "Documentos");
            var extension = Path.GetExtension(archivo.Archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaRelativa = Path.Combine("Archivos", "Documentos", nombreArchivo);
            var rutaCompleta = Path.Combine(carpetaDocumentos, nombreArchivo);

            try
            {
                // Verifica si el documento existe
                var documento = await _context.Documentos.FindAsync(idDocumento);
                if (documento == null)
                    return NotFound(new RespuestaDto<string>(false, "Documento no encontrado.", null));

                // Crea la carpeta si no existe
                if (!Directory.Exists(carpetaDocumentos))
                    Directory.CreateDirectory(carpetaDocumentos);

                // Guarda el nuevo archivo
                await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.Archivo.CopyToAsync(stream);
                }

                // Opcional: eliminar el archivo anterior si existe
                if (!string.IsNullOrEmpty(documento.RutaArchivo))
                {
                    var rutaAnterior = Path.Combine(Directory.GetCurrentDirectory(), documento.RutaArchivo);
                    if (System.IO.File.Exists(rutaAnterior))
                        System.IO.File.Delete(rutaAnterior);
                }

                // Actualiza la ruta del documento
                documento.RutaArchivo = rutaRelativa;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new RespuestaDto<string>(true, "Actualización exitosa.", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] DocumentoRegistroDto input)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            string? rutaRelativa = null;

            try
            {
                // Validación previa: verificar si el tipo de documento existe
                //var tipoDocumento = await _context.TiposDocumentos
                //    .FirstOrDefaultAsync(t => t.IdTipoDocumento == input.IdTipoDocumento && t.Estado == true);

                var clasificacion = await _context.Clasificacions.FirstOrDefaultAsync(c => c.IdClasificacion == input.IdClasificacion && c.Estado == true);
                string carpetaTipo = string.Empty;
                string baseCarpeta = string.Empty;
                if (clasificacion != null)
                {
                    //return BadRequest(new RespuestaDto<string>(false, "La Clasificacion especificado no existe o está inactivo.", null));
                    carpetaTipo = clasificacion.Nombre.ToString();

                    baseCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "Archivos", "Documentos", carpetaTipo);

                    if (!Directory.Exists(baseCarpeta))
                        Directory.CreateDirectory(baseCarpeta);
                }


                if (input.Archivo != null && input.Archivo.Length > 0)
                {
                    var extension = Path.GetExtension(input.Archivo.FileName);
                    var nombreArchivo = $"{Guid.NewGuid()}{extension}";

                    rutaRelativa = Path.Combine("Archivos", "Documentos", carpetaTipo, nombreArchivo);
                    var rutaCompleta = Path.Combine(baseCarpeta, nombreArchivo);

                    await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await input.Archivo.CopyToAsync(stream);
                    }
                }

                var documento = new Documento
                {
                    Titulo = input.Titulo,
                    Descripcion = input.Descripcion,
                    RutaArchivo = rutaRelativa ?? string.Empty,
                    IdTipoDocumento = input.IdTipoDocumento,
                    IdClasificacion = input.IdClasificacion,
                    Estado = true,
                    IdEstado = input.IdEstado,
                    IdUsuarioCreador = input.IdUsuarioCreador,
                    IdUsuarioAsignado = input.IdUsuarioAsignado,
                    IdDepartamento = input.IdDepartamento,
                    IdDocumentoRelacionado = input.IdDocumentoRelacionado,
                    FechaCreacion = DateTime.UtcNow.AddHours(-5)
                };               

                _context.Documentos.Add(documento);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (input.IdDocumentoRelacionado != null && input.IdDocumentoRelacionado > 0)
                {
                    var data = new DocumentoChangeStateDto()
                    {
                        IdDocumento = (int)input.IdDocumentoRelacionado,
                        IdEstado = (int)EstadoDocumento.Archivado,
                        IdUsuarioModifica = (int)input.IdUsuarioCreador
                    };
                    await ChangeEstateDocument(data);
                }

                if (input.IdUsuarioCreador.HasValue)
                {
                    await RegistrarHistorialDocumentoAsync(
                        idDocumento: documento.IdDocumento,
                        idUsuario: input.IdUsuarioCreador.Value,
                        accion: input.Archivo != null ? "Registro de documento con archivo" : "Registro de documento sin archivo",
                        comentarios: input.Comentarios ?? "Documento registrado."
                    );
                }

                return Ok(new RespuestaDto<string>(true, "Documento registrado exitosamente.", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("ChangeUser")]
        public async Task<IActionResult> ChangeUser([FromBody] DocumentoChangeUserDto data)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var doc = new Documento { IdDocumento = data.IdDocumento };
                _context.Documentos.Attach(doc);
                doc.IdUsuarioAsignado = data.IdUsuarioAsignado;
                doc.IdEstado = (int)EstadoDocumento.Pendiente;

                _context.Entry(doc).Property(x => x.IdUsuarioAsignado).IsModified = true;
                _context.Entry(doc).Property(x => x.IdEstado).IsModified = true;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();
                var user = new Usuario { IdUsuario = data.IdUsuarioAsignado };
                _context.Usuarios.Attach(user);
                await RegistrarHistorialDocumentoAsync(
                    idDocumento: data.IdDocumento,
                    idUsuario: idUsuarioToken,
                    accion: $"Reasignado a {user.Nombres}",
                    comentarios: $"Asigando a UsuarioID {data.IdUsuarioAsignado} Correctamente."
                );

                return Ok(new RespuestaDto<string>(true, $"Documento {data.IdDocumento} actualizado correctamente.", null));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("GetDocumentsPendingByUser/{idUsuarioAsignado}")]
        public async Task<IActionResult> GetDocumentsPendingByUser(int idUsuarioAsignado)
        {
            if (idUsuarioAsignado < 1)
                return BadRequest(new RespuestaDto<string>(false, "Debe enviar un ID válido.", null));
            
            try
            {
                var query = _context.Documentos
                    .Where(d => d.IdUsuarioAsignado == idUsuarioAsignado && d.IdEstado == 2) // pendientes
                    .Include(d => d.IdClasificacionNavigation)
                    .Include(d => d.IdTipoDocumentoNavigation)
                    .Include(d => d.IdEstadoNavigation)
                    .Include(d => d.IdDepartamentoNavigation)
                    .Include(d => d.IdUsuarioAsignadoNavigation)
                    .AsQueryable();

                var documentos = await query
                    .Select(DocumentoSelector)
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{documentos.Count} documentos pendientes encontrados.", documentos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("GetAllDocumentsByUser/{idUsuarioAsignado}")]
        public async Task<IActionResult> GetAllDocumentsByUser(int idUsuarioAsignado)
        {
            if (idUsuarioAsignado < 1)
                return BadRequest(new RespuestaDto<string>(false, "Debe enviar un ID válido.", null));

            try
            {
                var query = _context.Documentos
                    .Where(d => d.IdUsuarioAsignado == idUsuarioAsignado)
                    .Include(d => d.IdClasificacionNavigation)
                    .Include(d => d.IdTipoDocumentoNavigation)
                    .Include(d => d.IdEstadoNavigation)
                    .Include(d => d.IdDepartamentoNavigation)
                    .Include(d => d.IdUsuarioAsignadoNavigation)
                    .AsQueryable();

                var documentos = await query
                    .Select(DocumentoSelector)
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{documentos.Count} documentos encontrados.", documentos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("GetDocumentsByFilter")]
        public async Task<IActionResult> GetDocumentsByFilter([FromQuery] DocumentoFiltroDto filtroDto)
        {
            var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();

            try
            {
                var query = _context.Documentos
                    .Include(d => d.IdClasificacionNavigation)
                    .Include(d => d.IdTipoDocumentoNavigation)
                    .Include(d => d.IdEstadoNavigation)
                    .Include(d => d.IdDepartamentoNavigation)
                    .Include(d => d.IdUsuarioAsignadoNavigation)
                    .OrderByDescending(d => d.IdDocumento)
                    .AsQueryable();

                // Filtro por rol (solo si no es admin)
                if (idRol != 1)
                    query = query.Where(d => d.IdUsuarioAsignado == idUsuarioToken);

                // Filtros condicionales
                if (filtroDto.IdEstado > 0)
                    query = query.Where(d => d.IdEstado == filtroDto.IdEstado);

                if (filtroDto.IdDepartamento > 0)
                    query = query.Where(d => d.IdDepartamento == filtroDto.IdDepartamento);

                if (filtroDto.IdClasificacion > 0)
                    query = query.Where(d => d.IdClasificacion == filtroDto.IdClasificacion);

                if (!string.IsNullOrWhiteSpace(filtroDto.TextoBusqueda))
                {
                    string texto = filtroDto.TextoBusqueda.Trim().ToLower();
                    query = query.Where(d =>
                        d.Titulo.ToLower().Contains(texto));
                }

                var total = await query.CountAsync();

                var documentos = await query
                    .OrderByDescending(d => d.IdDocumento)
                    .Skip((filtroDto.PageNumber - 1) * filtroDto.PageSize)
                    .Take(filtroDto.PageSize)
                    .Select(DocumentoSelector)
                    .ToListAsync();

                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = $"{documentos.Count} documentos encontrados.",
                    Datos = new
                    {
                        Total = total,
                        PaginaActual = filtroDto.PageNumber,
                        TamañoPagina = filtroDto.PageSize,
                        Documentos = documentos
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>
                {
                    Exito = false,
                    Mensaje = "Error interno del servidor.",
                    Datos = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("GetDocumentsReporterByFilter")]
        public async Task<IActionResult> GetDocumentsReporterByFilter([FromQuery] ReporteDocumentoFiltroDto filtroDto)
        {
            var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();

            try
            {
                var query = _context.Documentos
                    .Include(d => d.IdClasificacionNavigation)
                    .Include(d => d.IdTipoDocumentoNavigation)
                    .Include(d => d.IdEstadoNavigation)
                    .Include(d => d.IdDepartamentoNavigation)
                    .Include(d => d.IdUsuarioAsignadoNavigation)
                    .OrderByDescending(d => d.IdDocumento)
                    .AsQueryable();

                // Filtro por rol (solo si no es admin)
                if (idRol == 2)
                    query = query.Where(d => d.IdUsuarioAsignado == idUsuarioToken);
                else if (idRol == 3)
                    query = query.Where(d => d.IdUsuarioAsignadoNavigation.IdRol == 3);

                // Filtros condicionales
                if (filtroDto.IdEstado > 0)
                    query = query.Where(d => d.IdEstado == filtroDto.IdEstado);

                query = query.Where(d => d.FechaCreacion >= filtroDto.FechaInicio);
                query = query.Where(d => d.FechaCreacion <= filtroDto.FechaFin);

                if (filtroDto.IdClasificacion > 0)
                    query = query.Where(d => d.IdClasificacion == filtroDto.IdClasificacion);

                var total = await query.CountAsync();

                var documentos = await query
                    .OrderByDescending(d => d.IdDocumento)
                    .Skip((filtroDto.PageNumber - 1) * filtroDto.PageSize)
                    .Take(filtroDto.PageSize)
                    .Select(DocumentoSelector)
                    .ToListAsync();

                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = $"{documentos.Count} documentos encontrados.",
                    Datos = new
                    {
                        Total = total,
                        PaginaActual = filtroDto.PageNumber,
                        TamañoPagina = filtroDto.PageSize,
                        Documentos = documentos
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>
                {
                    Exito = false,
                    Mensaje = "Error interno del servidor.",
                    Datos = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("GetHistorialDocumentbyId/{idDocumento}")]
        public async Task<IActionResult> GetHistorialDocumentbyId(int idDocumento)
        {
            if (idDocumento < 1)
                return BadRequest(new RespuestaDto<string>(false, "Debe enviar un ID válido.", null));

            try
            {
                var query = _context.HistorialDocumentos
                    .Where(d => d.IdDocumento == idDocumento)
                    .Include(d => d.IdDocumentoNavigation)
                    .Include(d => d.IdUsuarioNavigation)
                    .OrderByDescending(d => d.FechaAccion)
                    .AsQueryable();

                var documentos = await query
                    .Select(d => new HistorialDocumentoDto
                    {
                        FechaAccion = d.FechaAccion,
                        Accion = d.Accion,
                        Comentario = d.Comentarios,
                        Responsable = $"{d.IdUsuarioNavigation.Nombres} {d.IdUsuarioNavigation.Apellidos}"
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{documentos.Count} Archivos de Historial encontrados.", documentos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

    }
}
