using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Repositorios;

public interface IFaseRepo : IRepositorioABMAnidado<Fase, int>
{
    Task CambiarTipo(int padreId, int id, TipoDeFaseEnum nuevoTipo);
    Task DecrementarNumeroDeFasesPosteriores(int torneoId, int numeroEliminado);

    /// <summary>Filas afectadas (0 si no existe la fase en ese torneo).</summary>
    Task<int> ActualizarEsVisibleEnApp(int torneoId, int faseId, bool esVisibleEnApp);
}
