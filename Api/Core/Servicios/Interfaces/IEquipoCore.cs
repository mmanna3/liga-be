using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IEquipoCore : ICoreABM<EquipoDTO>
{
    Task<ObtenerNombreEquipoDTO> ObtenerNombrePorCodigoAlfanumerico(string codigoAlfanumerico);
    Task<ObtenerClubDTO> ObtenerClubPorCodigoAlfanumericoDelEquipo(string codigoAlfanumerico);
}