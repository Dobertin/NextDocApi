namespace NextDocApi.Helper
{
    public enum EstadoDocumento
    {   
        Recibido = 1,
        Pendiente,
        Procesado,
        Archivado,
        Eliminado,
        Enviado
    }
    public enum Roles
    {
        Administrador =1,
        Asistente,
        MesaPartes
    }
}
