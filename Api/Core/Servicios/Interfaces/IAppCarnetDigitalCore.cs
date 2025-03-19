using Api.Core.DTOs.AppCarnetDigital;

namespace Api.Core.Servicios.Interfaces;

public interface IAppCarnetDigitalCore
{
    Task<EquiposDelDelegadoDTO> ObtenerEquiposPorUsuarioDeDelegado(string usuario);
}