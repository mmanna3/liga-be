using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface ILogsCore
{
    BusquedaLogsDTO Buscar(string texto, int dias = 14, int maxResultados = 200);
    IReadOnlyList<LogArchivoDTO> ListarArchivos(int? dias = null);
}
