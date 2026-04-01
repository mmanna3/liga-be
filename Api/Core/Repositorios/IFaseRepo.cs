using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IFaseRepo : IRepositorioABMAnidado<Fase, int>
{
    Task CambiarTipo(int padreId, int id, TipoDeFaseEnum nuevoTipo);
    Task DecrementarNumeroDeFasesPosteriores(int torneoId, int numeroEliminado);
}
