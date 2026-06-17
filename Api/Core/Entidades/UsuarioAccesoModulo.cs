using Api.Core.Enums;

namespace Api.Core.Entidades;

public class UsuarioAccesoModulo : Entidad
{
    public int UsuarioId { get; set; }
    public virtual Usuario Usuario { get; set; } = null!;

    public ModuloSistema Modulo { get; set; }
    public NivelAcceso Nivel { get; set; }
}
