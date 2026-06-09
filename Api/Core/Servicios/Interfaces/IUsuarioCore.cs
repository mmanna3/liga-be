using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IUsuarioCore : ICoreABM<UsuarioAdminDTO>
{
    Task<bool> BlanquearClave(int id);
    Task<IEnumerable<RolDTO>> ListarRolesAsignables();
}
