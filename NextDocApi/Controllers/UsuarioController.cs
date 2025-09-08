using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;
        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("GetUserNumber/{idUsuario}")]
        public async Task<IActionResult> GetUserNumber(int idUsuario)
        {
            if (idUsuario <= 0)
                return BadRequest(new RespuestaDto<string>(false, "Debe enviar un ID válido.", null));

            try
            {
                var numero = await _context.Usuarios
                    .Where(u => u.IdUsuario == idUsuario)
                    .Select(u => u.NroWhatsapp)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(numero))
                {
                    return NotFound(new RespuestaDto<string>(false, "No se encontró el número de WhatsApp para este usuario.", null));
                }

                return Ok(new RespuestaDto<string>(true, "Número encontrado correctamente.", numero));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new RespuestaDto<string>(false, "Error interno del servidor.", ex.Message));
            }
        }

    }
}
