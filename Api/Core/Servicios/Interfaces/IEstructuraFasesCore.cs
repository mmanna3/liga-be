using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IEstructuraFasesCore
{
    Task PersistirEstructura(int torneoId, EstructuraFasesDTO dto);
}
