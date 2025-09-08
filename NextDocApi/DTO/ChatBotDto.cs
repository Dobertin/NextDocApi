namespace NextDocApi.DTO
{
    public class ChatBotQueryDto
    {
        public DateTime? desde { get; set; }
        public DateTime? hasta{ get; set; }

    }
    public class VerficarDocumentoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado {  get; set; } = string.Empty;
        public DateTime? UltimaModificacion { get; set; }
    }
    public class RecordatorioDocumentoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public int? DocumentoID { get; set; }
    }
    public class HistorialTramitesDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public DateTime? FechaAccion { get; set; }
    }
}
