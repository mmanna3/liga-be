using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IDniExpulsadoDeLaLigaRepo : IRepositorioABM<DniExpulsadoDeLaLiga>
{
    Task<bool> ExistePorDniAsync(int dni, CancellationToken cancellationToken = default);
}
