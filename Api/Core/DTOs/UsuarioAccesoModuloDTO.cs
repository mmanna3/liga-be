using Api.Core.Enums;

namespace Api.Core.DTOs;

public class UsuarioAccesoModuloDTO
{
    public ModuloSistema Modulo { get; set; }
    public NivelAcceso Nivel { get; set; }
}
