using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IFechaCore : ICoreABMAnidado<int, FechaDTO>
{
    Task<IEnumerable<FechaTodosContraTodosDTO>> CrearFechasTodosContraTodosMasivamente(int padreId,
        IEnumerable<FechaTodosContraTodosDTO> dtos);

    Task<FechaEliminacionDirectaDTO> CrearFechasEliminacionDirectaMasivamente(int padreId,
        FechaEliminacionDirectaDTO dto);

    Task ModificarMasivamente(int padreId, IEnumerable<FechaDTO> dtos);
    Task CargarResultados(int zonaId, int jornadaId, CargarResultadosDTO dto);

    /// <summary>
    /// Elimina todas las fechas de una zona de eliminación directa, con sus jornadas y partidos.
    /// </summary>
    Task BorrarFechasEliminacionDirectaMasivamente(int padreId);

    /// <summary>
    /// Elimina todas las fechas de una zona todos contra todos, con sus jornadas y partidos.
    /// </summary>
    Task BorrarFechasTodosContraTodosMasivamente(int padreId);

    Task CambiarVisibilidadEnApp(int zonaId, int fechaId, bool esVisibleEnApp);
}
