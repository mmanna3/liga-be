using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface ITorneoFaseRepo : IRepositorioABMAnidado<TorneoFase, int>
{
    Task CambiarTipo(int padreId, int id, TipoDeFaseEnum nuevoTipo);
    Task DecrementarNumeroDeFasesPosteriores(int torneoId, int numeroEliminado);
}
