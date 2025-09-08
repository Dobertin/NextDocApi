namespace NextDocApi.DTO
{
    public class DashboardQueryDto
    {
        public int IdPeriodo { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin {  get; set; }
    }
    public class DashboardDataDto
    {
        public DashboardCardsDto Cards { get; set; }
        public List<PieChartDto> PieChart { get; set; }
        public List<BarChartDto> BarChart { get; set; }
    }

    public class DashboardCardsDto
    {
        public int Recibidos { get; set; }
        public int Enviados { get; set; }
        public int CertificacionPresupuestal { get; set; }
        public int GastosIrogados { get; set; }
        public int NotasPresupuestales { get; set; }
        public int DocumentosHT { get; set; }
    }

    public class PieChartDto
    {
        public string Name { get; set; }
        public double Porcentaje { get; set; }
    }

    public class BarChartDto
    {
        public string TipoDocumento { get; set; }
        public int Total { get; set; }
    }

}
