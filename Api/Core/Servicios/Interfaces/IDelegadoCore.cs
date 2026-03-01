using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IDelegadoCore : ICoreABM<DelegadoDTO>
{
    Task<IEnumerable<DelegadoDTO>> ListarConFiltro(IList<EstadoDelegadoEnum> estados);
    Task<int> AprobarDelegadoEnElClub(int delegadoClubId);
    Task<bool> BlanquearClave(int id);
    Task<int> Eliminar(int id);
    Task<int> FicharDelegadoSoloConDniYClub(FicharDelegadoSoloConDniYClubDTO dto);
    Task<string> ObtenerNombreUsuarioDisponible(string nombre, string apellido);
    Task<ObtenerNombreUsuarioPorDniDTO> ObtenerNombreUsuarioPorDni(string dni);
}