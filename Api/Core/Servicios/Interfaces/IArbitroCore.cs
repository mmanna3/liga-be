using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IArbitroCore : ICoreABM<ArbitroDTO>
{
    Task<int> Registrar(RegistroArbitroDTO dto);
    Task<IEnumerable<ArbitroDTO>> ListarPendientes();
    Task<int> Aprobar(int id, AprobarArbitroDTO dto);
}
