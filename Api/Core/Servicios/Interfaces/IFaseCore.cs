using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IFaseCore : ICoreABMAnidado<int, FaseDTO>
{
    Task CambiarVisibilidadEnApp(int torneoId, int faseId, bool esVisibleEnApp);
}
