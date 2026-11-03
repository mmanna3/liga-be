using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface ITorneoAgrupadorCore : ICoreABM<TorneoAgrupadorDTO>
{
    Task<IReadOnlyList<TorneoAgrupadorParaArbitroDTO>> ListarParaArbitros();
}
