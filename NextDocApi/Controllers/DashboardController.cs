using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NextDocApi.Data;
using NextDocApi.DTO;

namespace NextDocApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        public DashboardController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [Authorize]
        [HttpGet("GetDataForCardsandGraphichs")]
        public async Task<IActionResult> GetDataForCardsandGraphichs([FromQuery] DashboardQueryDto filtroDto)
        {
            try
            {
                // Calcular fechas según el periodo
                DateTime? fechaInicio = filtroDto.FechaInicio;
                DateTime? fechaFin = filtroDto.FechaFin;

                if (filtroDto.IdPeriodo > 0)
                {
                    var hoy = DateTime.UtcNow.AddHours(-5).Date;

                    switch (filtroDto.IdPeriodo)
                    {
                        case 1: // Día
                            fechaInicio = hoy;
                            fechaFin = hoy.AddDays(1).AddTicks(-1);
                            break;
                        case 2: // Semana
                            var lunes = hoy.AddDays(-(int)hoy.DayOfWeek + (int)DayOfWeek.Monday);
                            fechaInicio = lunes;
                            fechaFin = lunes.AddDays(7).AddTicks(-1);
                            break;
                        case 3: // Mes
                            fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
                            fechaFin = fechaInicio.Value.AddMonths(1).AddTicks(-1);
                            break;
                        case 4: // Año
                            fechaInicio = new DateTime(hoy.Year, 1, 1);
                            fechaFin = fechaInicio.Value.AddYears(1).AddTicks(-1);
                            break;
                    }
                }

                // Consulta base
                var baseQuery = _context.Documentos
                    .Include(d => d.IdTipoDocumentoNavigation)
                    .Include(d => d.IdEstadoNavigation)
                    .Where(d => d.Estado == true);

                if (fechaInicio.HasValue && fechaFin.HasValue)
                    baseQuery = baseQuery.Where(d => d.FechaCreacion >= fechaInicio && d.FechaCreacion <= fechaFin);

                // Conteo para tarjetas (si no hay datos se devolverá 0 por defecto)
                var recibidos = await baseQuery.CountAsync(d => d.IdEstado == 1);
                var enviados = await baseQuery.CountAsync(d => d.IdEstado == 6);
                var certPresupuestal = await baseQuery.CountAsync(d => d.IdClasificacion == 2);
                var gastosIrogados = await baseQuery.CountAsync(d => d.IdClasificacion == 3);
                var notasPresupuestales = await baseQuery.CountAsync(d => d.IdClasificacion == 5);
                var documentosHT = await baseQuery.CountAsync(d => d.IdClasificacion == 7);

                // Gráfico de pastel
                var atendidos = await baseQuery.CountAsync(d => d.IdEstado == 3);
                var enProceso = await baseQuery.CountAsync(d => d.IdEstado == 2);
                var totalPie = atendidos + enProceso;

                var pieChart = totalPie == 0
                    ? new List<PieChartDto>
                    {
                new PieChartDto { Name = "En Proceso", Porcentaje = 0 },
                new PieChartDto { Name = "Atendidos", Porcentaje = 0 }
                    }
                    : new List<PieChartDto>
                    {
                new PieChartDto { Name = "En Proceso", Porcentaje = Math.Round((double)enProceso * 100 / totalPie, 2) },
                new PieChartDto { Name = "Atendidos", Porcentaje = Math.Round((double)atendidos * 100 / totalPie, 2) }
                    };

                // Gráfico de barras por tipo de documento
                var barChart = await baseQuery
                    .GroupBy(d => d.IdTipoDocumentoNavigation.NombreTipo)
                    .Select(g => new BarChartDto
                    {
                        TipoDocumento = g.Key,
                        Total = g.Count()
                    })
                    .ToListAsync();

                // En caso de que no haya datos para el gráfico, retornar lista vacía
                barChart ??= new List<BarChartDto>();

                // Armar respuesta
                var response = new DashboardDataDto
                {
                    Cards = new DashboardCardsDto
                    {
                        Recibidos = recibidos,
                        Enviados = enviados,
                        CertificacionPresupuestal = certPresupuestal,
                        GastosIrogados = gastosIrogados,
                        NotasPresupuestales = notasPresupuestales,
                        DocumentosHT = documentosHT
                    },
                    PieChart = pieChart,
                    BarChart = barChart
                };

                return Ok(new RespuestaDto<object>
                {
                    Exito = true,
                    Mensaje = "Datos del dashboard generados correctamente.",
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
    }
}
