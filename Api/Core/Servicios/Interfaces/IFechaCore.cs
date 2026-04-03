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
}
