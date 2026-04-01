using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IZonaCore : ICoreABMAnidado<int, ZonaDTO>
{
    Task<IEnumerable<ZonaDTO>> CrearMasivamente(int padreId, IEnumerable<ZonaDTO> dtos);
    Task ModificarMasivamente(int padreId, IEnumerable<ZonaDTO> dtos);
}
