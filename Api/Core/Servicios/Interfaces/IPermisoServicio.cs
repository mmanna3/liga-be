using Api.Core.DTOs;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IPermisoServicio
{
    bool EsSuperAdministrador();
    NivelAcceso? ObtenerNivel(ModuloSistema modulo);
    bool TieneAcceso(ModuloSistema modulo);
    bool PuedeEditar(ModuloSistema modulo);
    bool PuedeEliminar(ModuloSistema modulo);
    IReadOnlyList<UsuarioAccesoModuloDTO> ObtenerPermisosDelUsuario();
}
