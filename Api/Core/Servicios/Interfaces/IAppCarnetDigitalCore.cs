using Api.Core.DTOs.AppCarnetDigital;

namespace Api.Core.Servicios.Interfaces;

public interface IAppCarnetDigitalCore
{
    Task<List<EquiposDelDelegadoDTO>> ObtenerEquiposPorUsuarioDeDelegado(string usuario);
    Task<ICollection<CarnetDigitalDTO>> Carnets(int equipoId);
    Task<ICollection<CarnetDigitalPendienteDTO>> JugadoresPendientes(int equipoId);
    Task<ICollection<CarnetDigitalDTO>> CarnetsPorCodigoAlfanumerico(string codigoAlfanumerico);
}