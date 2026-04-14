using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class Configuracion : Entidad
{
    public HabilitacionFichaje HabilitacionFichaje { get; set; } = null!;
    public required int HabilitacionFichajeId { get; set; }
}