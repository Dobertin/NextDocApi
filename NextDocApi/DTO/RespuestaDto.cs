namespace NextDocApi.DTO
{
    public class RespuestaDto<T>
    {
        public RespuestaDto()
        {
            Mensaje = "";
        }

        public RespuestaDto(bool estado, string mensaje, T data)
        {
            Exito = estado;
            Mensaje = mensaje;
            Datos = data;
        }

        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T? Datos { get; set; }
    }
}
