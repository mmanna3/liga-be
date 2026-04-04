using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoAgrupadorRepo : IRepositorioABM<TorneoAgrupador>
{
    Task<IReadOnlyList<InformacionInicialAgrupadorDTO>> ListarInformacionInicialParaAppAsync(
        CancellationToken cancellationToken = default);
}
