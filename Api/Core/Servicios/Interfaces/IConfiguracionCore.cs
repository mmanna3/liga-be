using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IConfiguracionCore : ICoreABM<ConfiguracionDTO>
{
    Task<bool> FichajeEstaHabilitado();
}
