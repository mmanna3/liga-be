using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IFechaCore : ICoreABMAnidado<int, FechaDTO>
{
    Task<IEnumerable<FechaTodosContraTodosDTO>> CrearFechasTodosContraTodosMasivamente(int padreId,
        IEnumerable<FechaTodosContraTodosDTO> dtos);

    Task<IEnumerable<FechaEliminacionDirectaDTO>> CrearFechasEliminacionDirectaMasivamente(int padreId,
        IEnumerable<FechaEliminacionDirectaDTO> dtos);

    Task ModificarMasivamente(int padreId, IEnumerable<FechaDTO> dtos);
    Task CargarResultados(int zonaId, int jornadaId, CargarResultadosDTO dto);
}
