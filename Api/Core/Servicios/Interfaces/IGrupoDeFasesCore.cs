using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IGrupoDeFasesCore : ICoreABMAnidado<int, GrupoDeFasesDTO>
{
    Task CambiarVisibilidadEnApp(int torneoId, int grupoId, bool esVisibleEnApp);
}
