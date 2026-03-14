using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IFixtureAlgoritmoRepo : IRepositorioABM<FixtureAlgoritmo>
{
    Task EliminarFechasDelFixture(int fixtureAlgoritmoId);
}
