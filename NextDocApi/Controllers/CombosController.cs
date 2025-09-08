using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;
using NextDocApi.Helper;
using NextDocApi.Modelos;
using System.Linq;
using System.Security.Claims;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CombosController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public CombosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private (int IdUsuario, int IdRol) ObtenerUsuarioYRolDesdeToken()
        {
            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var idRol = int.Parse(User.FindFirst("IdRol")?.Value ?? "0"); // <-- usa el nuevo claim "IdRol"
            return (idUsuario, idRol);
        }

        [Authorize]
        [HttpGet("ObtenerClasificacion")]
        public async Task<IActionResult> ObtenerClasificacion()
        {
            try
            {
                var data = await _context.Clasificacions
                    .Where(c => c.Estado == true)
                    .Select(c => new ComboDto
                    {
                        Id = c.IdClasificacion,
                        Name = c.Nombre
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{data.Count} Objetos encontradas.", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("ObtenerEstadosDocumento")]
        public async Task<IActionResult> ObtenerEstadosDocumento()
        {
            try
            {
                var (idUsuarioToken, idRol) = ObtenerUsuarioYRolDesdeToken();

                IQueryable<ComboDto> query = _context.EstadosDocumentos
                    .Where(c => c.Estado == true)
                    .Select(c => new ComboDto
                    {
                        Id = c.IdEstado,
                        Name = c.NombreEstado
                    });

                if (idRol == (int)Roles.MesaPartes)
                {
                    query = query.Where(c => new[] { 1, 6, 4 }.Contains(c.Id));
                }
                else if (idRol == (int)Roles.Asistente)
                {
                    query = query.Where(c => new[] { 2, 3 }.Contains(c.Id));
                }
                else if (idRol == (int)Roles.Administrador)
                {
                    query = query.Where(c => new[] { 1, 2 }.Contains(c.Id));
                }

                var data = await query.ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{data.Count} objetos encontrados.", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }


        [Authorize]
        [HttpGet("ObtenerTipoDocumento")]
        public async Task<IActionResult> ObtenerTipoDocumento()
        {
            try
            {
                var data = await _context.TiposDocumentos
                    .Where(c => c.Estado == true)
                    .Select(c => new ComboDto
                    {
                        Id = c.IdTipoDocumento,
                        Name = c.NombreTipo
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{data.Count} Objetos encontradas.", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("ObtenerAreas")]
        public async Task<IActionResult> ObtenerAreas()
        {
            try
            {
                var data = await _context.Departamentos
                    .Where(c => c.Estado == true)
                    .Select(c => new ComboDto
                    {
                        Id = c.IdDepartamento,
                        Name = c.NombreDepartamento
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{data.Count} Objetos encontradas.", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("ObtenerComboDocumento/{nombreDoc}")]
        public async Task<IActionResult> ObtenerComboDocumento(string nombreDoc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreDoc))
                {
                    return BadRequest(new RespuestaDto<string>(false, "Debe proporcionar un nombre de documento válido.", null));
                }

                var nombreDocLower = nombreDoc.ToLower();

                var data = await _context.Documentos
                    .Where(d =>
                        d.Estado == true &&
                        d.IdEstado != 5 && // Excluye eliminados
                        d.Titulo.ToLower().Contains(nombreDocLower))
                    .Select(d => new ComboDto
                    {
                        Id = d.IdDocumento,
                        Name = d.Titulo
                    })
                    .ToListAsync();

                if (data.Count == 0)
                {
                    data.Add(new ComboDto
                    {
                        Id = 0,
                        Name = "No hay coincidencias"
                    });
                }

                return Ok(new RespuestaDto<object>(true, $"{data.Count} resultado(s) encontrado(s).", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("ObtenerUsuarioPorArea/{idArea}")]
        public async Task<IActionResult> ObtenerUsuarioPorArea(int idArea)
        {
            try
            {
                if (idArea < 1)
                {
                    return BadRequest(new RespuestaDto<string>(false, "Debe proporcionar un Area válida.", null));
                }

                var data = await _context.Usuarios
                    .Where(d => d.Estado == true &&
                        d.IdDepartamento == idArea)
                    .Select(d => new ComboDto
                    {
                        Id = d.IdUsuario,
                        Name = $"{d.Nombres} {d.Apellidos}"
                    })
                    .ToListAsync();

                if (data.Count == 0)
                {
                    data.Add(new ComboDto
                    {
                        Id = 0,
                        Name = "No hay coincidencias"
                    });
                }

                return Ok(new RespuestaDto<object>(true, $"{data.Count} resultado(s) encontrado(s).", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

        [Authorize]
        [HttpGet("ObtenerRoles")]
        public async Task<IActionResult> ObtenerRoles()
        {
            try
            {
                var data = await _context.Roles
                    .Where(c => c.Estado == true)
                    .Select(c => new ComboDto
                    {
                        Id = c.IdRol,
                        Name = c.NombreRol
                    })
                    .ToListAsync();

                return Ok(new RespuestaDto<object>(true, $"{data.Count} Objetos encontradas.", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }
        [Authorize]
        [HttpGet("ObtenerTodosLosUsuarios")]
        public async Task<IActionResult> ObtenerTodosLosUsuarios()
        {
            try
            {
                var data = await _context.Usuarios
                    .Where(d => d.Estado == true)
                    .Select(d => new ComboDto
                    {
                        Id = d.IdUsuario,
                        Name = $"{d.Nombres} {d.Apellidos}"
                    })
                    .ToListAsync();

                if (data.Count == 0)
                {
                    data.Add(new ComboDto
                    {
                        Id = 0,
                        Name = "No hay coincidencias"
                    });
                }

                return Ok(new RespuestaDto<object>(true, $"{data.Count} resultado(s) encontrado(s).", data));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }
    }
}
