namespace Api.Core.Servicios.Interfaces;

public interface IPublicoCore
{
    Task<bool> ElDniEstaFichado(string dni);
}