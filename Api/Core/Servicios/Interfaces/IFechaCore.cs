using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IFechaCore : ICoreABMAnidado<int, FechaDTO>
{
    Task<IEnumerable<FechaDTO>> CrearMasivamente(int padreId, IEnumerable<FechaDTO> dtos);
    Task ModificarMasivamente(int padreId, IEnumerable<FechaDTO> dtos);
}
