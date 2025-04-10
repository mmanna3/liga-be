using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IDelegadoCore : ICoreABM<DelegadoDTO>
{
    Task<bool> BlanquearClave(int id);
    Task<int> Eliminar(int id);
}