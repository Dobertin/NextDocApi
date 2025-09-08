using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NextDocApi.Data;
using NextDocApi.DTO;
using NextDocApi.Modelos;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new RespuestaDto<List<string>>(false, "Errores de validación.", errores));
                }

                var usuario = _context.Usuarios
                    .Include(u=> u.IdRolNavigation)
                    .FirstOrDefault(u => u.Email == login.Email && u.Estado == true);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(login.Password, usuario.PasswordHash))
                {
                    return Unauthorized(new RespuestaDto<string>
                    {
                        Exito = false,
                        Mensaje = "Credenciales inválidas",
                        Datos = null
                    });
                }

                var token = GenerarToken(usuario);

                var resultado = new
                {
                    Token = token,
                    Usuario = new
                    {
                        usuario.IdUsuario,
                        usuario.Nombres,
                        usuario.Apellidos,
                        usuario.Email,
                        usuario.IdRolNavigation.NombreRol,
                        usuario.IdRol,
                        usuario.IdDepartamento
                    }
                };

                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = "Login exitoso",
                    Datos = resultado
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
        [HttpGet("me")]
        public IActionResult GetUsuarioActual()
        {
            try
            {
                var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var usuario = _context.Usuarios
                    .Where(u => u.IdUsuario == idUsuario && u.Estado == true)
                    .Select(u => new
                    {
                        u.IdUsuario,
                        u.Nombres,
                        u.Apellidos,
                        u.Email,
                        u.IdRol,
                        u.IdDepartamento
                    })
                    .FirstOrDefault();

                if (usuario == null)
                {
                    return NotFound(new RespuestaDto<string>
                    {
                        Exito = false,
                        Mensaje = "Usuario no encontrado",
                        Datos = null
                    });
                }

                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = "Usuario encontrado",
                    Datos = usuario
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>
                {
                    Exito = false,
                    Mensaje = "Error interno del servidor",
                    Datos = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new RespuestaDto<List<string>>(false, "Errores de validación.", errores));
                }

                if (_context.Usuarios.Any(u => u.Email == dto.Email))
                    return BadRequest(new RespuestaDto<string>(false, "El correo ya está registrado.", null));

                if (!_context.Roles.Any(r => r.IdRol == dto.IdRol))
                    return BadRequest(new RespuestaDto<string>(false, "Rol no válido.", null));

                var nuevoUsuario = new Usuario
                {
                    Nombres = dto.Nombres,
                    Apellidos = dto.Apellidos,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    IdRol = dto.IdRol,
                    IdDepartamento = dto.IdDepartamento,
                    Estado = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();

                transaction.Commit();
                return Ok(new RespuestaDto<string>(true, "Usuario registrado correctamente.", null));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, new RespuestaDto<string>
                {
                    Exito = false,
                    Mensaje = "Error interno al registrar el usuario.",
                    Datos = ex.Message
                });
            }
        }

        private string GenerarToken(Usuario usuario)
        {
            var jwtConfig = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                  {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.IdRolNavigation.NombreRol), // nombre del rol
                new Claim("IdRol", usuario.IdRol.ToString())                    // <- nuevo claim
            };


            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
