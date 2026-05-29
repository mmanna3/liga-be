using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ISponsorWebPublicaRepo : IRepositorioABM<SponsorWebPublica>
{
    Task<int> ObtenerProximoOrdenAsync(CancellationToken cancellationToken = default);
}
