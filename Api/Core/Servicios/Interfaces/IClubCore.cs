using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IClubCore : ICoreABM<ClubDTO>
{
    Task<IEnumerable<ClubListadoDTO>> Listado();
    Task<int> CambiarEscudo(int clubId, string imagenBase64);
}