using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IPublicoCore
{
    Task<bool> ElDniEstaFichado(string dni);
    Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto);
}