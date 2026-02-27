using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IDelegadoCore : ICoreABM<DelegadoDTO>
{
    Task<int> Aprobar(AprobarDelegadoDTO dto);
    Task<bool> BlanquearClave(int id);
    Task<int> Eliminar(int id);
    Task<int> FicharDelegadoSoloConDniYClub(FicharDelegadoSoloConDniYClubDTO dto);
}