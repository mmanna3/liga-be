using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface ITorneoFechaCore : ICoreABMAnidado<int, TorneoFechaDTO>
{
    Task<IEnumerable<TorneoFechaDTO>> CrearMasivamente(int padreId, IEnumerable<TorneoFechaDTO> dtos);
    Task ModificarMasivamente(int padreId, IEnumerable<TorneoFechaDTO> dtos);
}
