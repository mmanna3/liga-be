using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface ITorneoZonaCore : ICoreABMAnidado<int, TorneoZonaDTO>
{
    Task<IEnumerable<TorneoZonaDTO>> CrearMasivamente(int padreId, IEnumerable<TorneoZonaDTO> dtos);
    Task ModificarMasivamente(int padreId, IEnumerable<TorneoZonaDTO> dtos);
}
