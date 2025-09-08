using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;
using NextDocApi.Helper;
using NextDocApi.Modelos;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdministracionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdministracionController(AppDbContext context)
        {
            _context = context;
        }
        private (int IdUsuario, int IdRol) ObtenerUsuarioYRolDesdeToken()
        {
            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var idRol = int.Parse(User.FindFirst("IdRol")?.Value ?? "0"); // <-- usa el nuevo claim "IdRol"
            return (idUsuario, idRol);
        }

        #region Roles
        [Authorize]
        [HttpGet("roles/listar")]
        public async Task<IActionResult> ListarRoles()
        {
            try
            {
                var lista = await _context.Roles
                    .Select(x=> new RolDto { 
                        IdRol = x.IdRol,
                        NombreRol = x.NombreRol,
                        Estado = x.Estado
                }).ToListAsync() ;
                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = "Roles obtenidos correctamente.",
                    Datos = lista
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al listar roles", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("roles/crear")]
        public async Task<IActionResult> CrearRol([FromBody] RolDto rol)
        {
            try
            {
                var data = new Role
                {
                    IdRol = rol.IdRol,
                    NombreRol = rol.NombreRol,
                    Estado = rol.Estado
                };
                await _context.Roles.AddAsync(data);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Rol creado correctamente", Datos = rol });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al crear rol", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("roles/actualizar/{id}")]
        public async Task<IActionResult> ActualizarRol(int id, [FromBody] RolDto rol)
        {
            try
            {
                var doc = new Role { IdRol = id };
                _context.Roles.Attach(doc);
                doc.NombreRol = rol.NombreRol;
                doc.Estado = rol.Estado;

                _context.Entry(doc).Property(x => x.NombreRol).IsModified = true;
                _context.Entry(doc).Property(x => x.Estado).IsModified = true;

                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Rol actualizado correctamente", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al actualizar rol", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("roles/eliminar/{id}")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            try
            {
                var entidad = await _context.Roles.FindAsync(id);
                if (entidad == null)
                    return NotFound(new RespuestaDto<object> { Exito = false, Mensaje = "Rol no encontrado", Datos = null });

                _context.Roles.Remove(entidad);
                await _context.SaveChangesAsync();

                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Rol eliminado correctamente", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al eliminar rol", Datos = ex.Message });
            }
        }
        #endregion

        #region Clasificacion
        [Authorize]
        [HttpGet("clasificacion/listar")]
        public async Task<IActionResult> ListarClasificacion()
        {
            try
            {
                var lista = await _context.Clasificacions
                    .Select(d=> new ClasificacionDto
                    {
                        IdClasificacion = d.IdClasificacion,
                        Nombre = d.Nombre,
                        Estado = d.Estado
                    })
                    .ToListAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Clasificaciones obtenidas", Datos = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al listar", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("clasificacion/crear")]
        public async Task<IActionResult> CrearClasificacion([FromBody] ClasificacionDto body)
        {
            try
            {
                var data = new Clasificacion
                {
                    Nombre = body.Nombre,
                    Estado = body.Estado,
                };
                await _context.Clasificacions.AddAsync(data);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Clasificación creada", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error al crear", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("clasificacion/actualizar/{id}")]
        public async Task<IActionResult> ActualizarClasificacion(int id, [FromBody] ClasificacionDto c)
        {
            try
            {
                var entity = new Clasificacion { IdClasificacion = id };
                _context.Clasificacions.Attach(entity);
                entity.Nombre = c.Nombre;
                entity.Estado = c.Estado;

                _context.Entry(entity).Property(x => x.Nombre).IsModified = true;
                _context.Entry(entity).Property(x => x.Estado).IsModified = true;

                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Actualizado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("clasificacion/eliminar/{id}")]
        public async Task<IActionResult> EliminarClasificacion(int id)
        {
            try
            {
                var entidad = await _context.Clasificacions.FindAsync(id);
                if (entidad == null)
                    return NotFound(new RespuestaDto<object> { Exito = false, Mensaje = "No encontrado", Datos = null });

                _context.Clasificacions.Remove(entidad);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Eliminado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }
        #endregion

        #region EstadosDocumento
        [Authorize]
        [HttpGet("estadosDocumento/listar")]
        public async Task<IActionResult> ListarEstadosDocumento()
        {
            try
            {
                var lista = await _context.EstadosDocumentos
                    .Select(d=> new EstadoDocumentoDto
                    {
                        IdEstado = d.IdEstado,
                        NombreEstado = d.NombreEstado,
                        Estado = d.Estado
                    })
                    .ToListAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Estados obtenidos", Datos = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("estadosDocumento/crear")]
        public async Task<IActionResult> CrearEstado([FromBody] EstadoDocumentoDto e)
        {
            try
            {
                var data = new EstadosDocumento
                {
                    NombreEstado = e.NombreEstado,
                    Estado = e.Estado
                };
                await _context.EstadosDocumentos.AddAsync(data);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Estado creado", Datos = e });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("estadosdocumento/actualizar/{id}")]
        public async Task<IActionResult> ActualizarEstadoDocumento(int id, [FromBody] EstadoDocumentoDto dto)
        {
            try
            {
                var entity = new EstadosDocumento { IdEstado = id };
                _context.EstadosDocumentos.Attach(entity);
                entity.NombreEstado = dto.NombreEstado;
                entity.Estado = dto.Estado;

                _context.Entry(entity).Property(x => x.NombreEstado).IsModified = true;
                _context.Entry(entity).Property(x => x.Estado).IsModified = true;

                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Actualizado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("estadosdocumento/eliminar/{id}")]
        public IActionResult EliminarEstadoDocumento(int id)
        {
            try
            {
                var estado = _context.EstadosDocumentos.Find(id);
                if (estado == null) return NotFound();
                _context.EstadosDocumentos.Remove(estado);
                _context.SaveChanges();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Eliminado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }
        #endregion

        #region Departamentos
        [Authorize]
        [HttpGet("departamento/listar")]
        public async Task<IActionResult> ListarDepartamentos()
        {
            try
            {
                var lista = await _context.Departamentos
                    .Select(d=> new DepartamentoDto
                    {
                        IdDepartamento = d.IdDepartamento,
                        NombreDepartamento = d.NombreDepartamento,
                        Estado = d.Estado
                    })
                    .ToListAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Datos Obtenidos", Datos = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("departamento/crear")]
        public async Task<IActionResult> CrearDepartamento([FromBody] DepartamentoDto dto)
        {
            try
            {
                var data = new Departamento
                {
                    IdDepartamento = dto.IdDepartamento,
                    NombreDepartamento = dto.NombreDepartamento,
                    Estado = dto.Estado
                };
                await _context.Departamentos.AddAsync(data);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Agregado con Exito", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("departamento/actualizar/{id}")]
        public async Task<IActionResult> ActualizarDepartamentoAsync(int id, [FromBody] DepartamentoDto dto)
        {
            try
            {
                var entity = new Departamento { IdDepartamento = id };
                _context.Departamentos.Attach(entity);
                entity.IdDepartamento = dto.IdDepartamento;
                entity.NombreDepartamento = dto.NombreDepartamento;
                entity.Estado = dto.Estado;

                _context.Entry(entity).Property(x => x.IdDepartamento).IsModified = true;
                _context.Entry(entity).Property(x => x.NombreDepartamento).IsModified = true;
                _context.Entry(entity).Property(x => x.Estado).IsModified = true;
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Actualizado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("departamento/eliminar/{id}")]
        public IActionResult EliminarDepartamento(int id)
        {
            try
            {
                var estado = _context.Departamentos.Find(id);
                if (estado == null) return NotFound();
                _context.Departamentos.Remove(estado);
                _context.SaveChanges();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Eliminado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }
        #endregion

        #region Tipos Documenmto
        [Authorize]
        [HttpGet("tiposdocumento/listar")]
        public async Task<IActionResult> ListarTiposDocumento()
        {
            try
            {
                var data = await _context.TiposDocumentos
                    .Select(d => new TipoDocumentoDto
                    {
                        IdTipoDocumento = d.IdTipoDocumento,
                        NombreTipo = d.NombreTipo,
                        Estado = d.Estado
                    })
                    .ToListAsync();
                return Ok(new RespuestaDto<object> { Datos= data, Mensaje = "Datos Obtenidos", Exito = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("tiposdocumento/crear")]
        public async Task<IActionResult> CrearTipoDocumentoAsync([FromBody] TipoDocumentoDto dto)
        {
            try
            {
                var data = new TiposDocumento
                {
                    NombreTipo = dto.NombreTipo,
                    Estado = dto.Estado
                };
                await _context.TiposDocumentos.AddAsync(data);
                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Agregado con Exito", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("tiposdocumento/actualizar/{id}")]
        public async Task<IActionResult> ActualizarTipoDocumentoAsync(int id, [FromBody] TipoDocumentoDto dto)
        {
            try
            {
                var entity = new TiposDocumento { IdTipoDocumento = id };
                _context.TiposDocumentos.Attach(entity);
                entity.NombreTipo = dto.NombreTipo;
                entity.Estado = dto.Estado;

                _context.Entry(entity).Property(x => x.NombreTipo).IsModified = true;
                _context.Entry(entity).Property(x => x.Estado).IsModified = true;

                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Actualizado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("tiposdocumento/eliminar/{id}")]
        public IActionResult EliminarTipoDocumento(int id)
        {
            try
            {
                var estado = _context.TiposDocumentos.Find(id);
                if (estado == null) return NotFound();
                _context.TiposDocumentos.Remove(estado);
                _context.SaveChanges();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Eliminado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }
        #endregion

        #region Usuarios
        [Authorize]
        [HttpGet("usuarios/listar")]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                var data = await _context.Usuarios
                    .Select(u=> new UsuarioDto
                    {
                        IdUsuario = u.IdUsuario,
                        Nombres = u.Nombres,
                        Apellidos = u.Apellidos,
                        Email = u.Email,
                        NroWhatsapp = u.NroWhatsapp,
                        IdRol = u.IdRol,
                        IdDepartamento = u.IdDepartamento,
                        Estado = u.Estado,
                    })
                    .ToListAsync();
                return Ok(new RespuestaDto<object> { Datos = data, Mensaje = "Datos Obtenidos", Exito = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("usuarios/actualizar/{id}")]
        public async Task<IActionResult> ActualizarUsuarioAsync(int id, [FromBody] UsuarioDto dto)
        {
            var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();
            try
            {
                if (idRol != (int)Roles.Administrador)
                {
                    if (idUsuarioToken != id)
                    {
                        return BadRequest(new RespuestaDto<object> { Exito = false, Mensaje = "Actualizado", Datos = null });
                    }
                }
                var usuario = new Usuario { IdUsuario = id };
                _context.Usuarios.Attach(usuario);

                usuario.Nombres = dto.Nombres;
                usuario.Apellidos = dto.Apellidos;
                usuario.Email = dto.Email;
                usuario.NroWhatsapp = dto.NroWhatsapp;
                usuario.IdRol = dto.IdRol;
                usuario.IdDepartamento = dto.IdDepartamento;
                usuario.Estado = dto.Estado;
                if (!string.IsNullOrEmpty(dto.Password))
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);                

                _context.Entry(usuario).Property(x => x.Nombres).IsModified = true;
                _context.Entry(usuario).Property(x => x.Apellidos).IsModified = true;
                _context.Entry(usuario).Property(x => x.Email).IsModified = true;
                _context.Entry(usuario).Property(x => x.NroWhatsapp).IsModified = true;
                _context.Entry(usuario).Property(x => x.IdRol).IsModified = true;
                _context.Entry(usuario).Property(x => x.IdDepartamento).IsModified = true;
                _context.Entry(usuario).Property(x => x.Estado).IsModified = true;
                if (!string.IsNullOrEmpty(dto.Password))
                    _context.Entry(usuario).Property(x => x.PasswordHash).IsModified = true;

                await _context.SaveChangesAsync();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Actualizado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("usuarios/eliminar/{id}")]
        public IActionResult EliminarUsuario(int id)
        {
            try
            {
                var estado = _context.Usuarios.Find(id);
                if (estado == null) return NotFound();
                _context.Usuarios.Remove(estado);
                _context.SaveChanges();
                return Ok(new RespuestaDto<object> { Exito = true, Mensaje = "Eliminado", Datos = null });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string> { Exito = false, Mensaje = "Error", Datos = ex.Message });
            }
        }
        #endregion
    }
}
