using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;
using System.Security.Claims;

namespace NextDocApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatBotController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChatBotController(AppDbContext context)
    {
        _context = context;
    }

    private int ObtenerIdUsuario() =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    
    [HttpGet("verificarDocumento/{consulta}")]
    public async Task<IActionResult> VerificarDocumento(string consulta)
    {
        try
        {
            var documento = await _context.Documentos
                .Include(d => d.IdEstadoNavigation)
                .Include(d => d.HistorialDocumentos.OrderByDescending(h => h.FechaAccion))
                .FirstOrDefaultAsync(d =>
                    d.Titulo.ToLower().Contains(consulta.ToLower()) || d.Descripcion.ToLower().Contains(consulta.ToLower()));

            if (documento == null)
                return NotFound(new RespuestaDto<string>
                {
                    Exito = false,
                    Mensaje = "No se encontró ningún documento con ese criterio.",
                    Datos = null
                });

            var ultimaModificacion = documento.HistorialDocumentos.FirstOrDefault()?.FechaAccion;

            var response = new VerficarDocumentoDto
            {
                Titulo = documento.Titulo,
                Descripcion = documento.Descripcion,
                Estado = documento.IdEstadoNavigation?.NombreEstado,
                UltimaModificacion = ultimaModificacion
            };

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = response
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

    [HttpGet("recordatorioPendientes")]
    public async Task<IActionResult> RecordatorioPendientes()
    {
        try
        {
            int idUsuario = ObtenerIdUsuario();

            var pendientes = await _context.Documentos
                .Where(d => d.IdUsuarioAsignado == idUsuario && d.IdEstado == 2)
                .Select(d => new RecordatorioDocumentoDto
                {
                   Titulo = d.Titulo,
                   DocumentoID= d.IdDocumento
                })
                .ToListAsync();

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = pendientes
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

    [HttpGet("historialTramites")]
    public async Task<IActionResult> HistorialTramites([FromQuery] ChatBotQueryDto data)
    {
        try
        {
            int idUsuario = ObtenerIdUsuario();

            var historial = await _context.HistorialDocumentos
                .Where(h => h.IdUsuario == idUsuario && h.FechaAccion >= data.desde && h.FechaAccion <= data.hasta)
                .Include(h => h.IdDocumentoNavigation)
                .Select(h => new HistorialTramitesDto
                {
                    Titulo = h.IdDocumentoNavigation.Titulo,
                    Accion = h.Accion,
                    FechaAccion = h.FechaAccion
                })
                .OrderByDescending(h => h.FechaAccion)
                .ToListAsync();

            if (historial == null || historial.Count == 0)
                return Ok(new RespuestaDto<object>
                {
                    Exito = false,
                    Mensaje = "No Se encontraron Resultados",
                    Datos = null
                });

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = historial
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

    [HttpGet("buscarDocumento/{filtro}")]
    public async Task<IActionResult> BuscarTitulos(string filtro)
    {
        try
        {
            if (string.IsNullOrEmpty(filtro.Trim()))
            {
                return BadRequest(new RespuestaDto<object>
                {
                    Exito = false,
                    Mensaje = "No debe enviar una cadena Vacia.",
                    Datos = null
                });
            }
            var titulos = await _context.Documentos
                .Where(d => d.Titulo.ToLower().Contains(filtro.ToLower()))
                .Select(d => d.Titulo)
                .Take(10) // Limitar resultados si se desea
                .ToListAsync();

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = titulos
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

    [HttpGet("DocumentosProgramados")]
    public async Task<IActionResult> BuscarDocumentosProgramados()
    {
        try
        {
            var hoy = DateTime.UtcNow.Date;

            var documentos = await _context.Documentos
                .Where(d => d.IdEstado == 2)
                .ToListAsync();

            var listaComentarios = new List<string>();

            foreach (var d in documentos)
            {
                int diasTranscurridos = DiasSinDomingos((DateTime)d.FechaCreacion, hoy);
                DateTime fechaVencimiento = CalcularFechaVencimiento((DateTime)d.FechaCreacion);

                if (diasTranscurridos < 1)
                {
                    // no se incluye
                    continue;
                }
                else if (diasTranscurridos >= 1 && diasTranscurridos < 2)
                {
                    listaComentarios.Add($"{d.Titulo} por vencer, vence el día {fechaVencimiento:dddd}");
                }
                else if (diasTranscurridos >= 2)
                {
                    listaComentarios.Add($"{d.Titulo} vencido el día {fechaVencimiento:dddd}");
                }
            }

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = listaComentarios
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

    [HttpGet("ResumenSemanal")]
    public async Task<IActionResult> ResumenSemanal()
    {
        try
        {
            var hoy = DateTime.UtcNow.Date;
            var haceUnaSemana = hoy.AddDays(-7);

            var resumen = await _context.Documentos
                .Where(d => d.Estado == true && d.FechaCreacion >= haceUnaSemana && d.FechaCreacion <= hoy)
                .GroupBy(d => new { d.IdEstado, d.IdEstadoNavigation.NombreEstado })
                .Select(g => new
                {
                    Estado = g.Key.NombreEstado,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            var listaComentarios = resumen
                .Select(r => $"{r.Cantidad} documentos con estado {r.Estado}")
                .ToList();

            return Ok(new RespuestaDto<object>
            {
                Exito = true,
                Mensaje = "Petición completada.",
                Datos = listaComentarios
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

    private int DiasSinDomingos(DateTime inicio, DateTime fin)
    {
        int dias = 0;
        var fecha = inicio.Date;

        while (fecha < fin.Date)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek != DayOfWeek.Sunday)
                dias++;
        }

        return dias;
    }
    private DateTime CalcularFechaVencimiento(DateTime fechaCreacion)
    {
        int diasAgregados = 0;
        var fecha = fechaCreacion;

        while (diasAgregados < 3)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek != DayOfWeek.Sunday)
                diasAgregados++;
        }

        return fecha;
    }

}
