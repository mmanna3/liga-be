using System.ComponentModel.DataAnnotations.Schema;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;

namespace Api.Core.Entidades;

public class Jornada : Entidad
{
    public virtual Fecha Fecha { get; set; } = null!;

    [ForeignKey("Fecha")]
    public required int FechaId { get; set; }

    public required bool ResultadosVerificados { get; set; }
}

public class JornadaNormal : Jornada
{
    public required int LocalEquipoId { get; set; }
    public virtual Equipo LocalEquipo { get; set; } = null!;
    public required int VisitanteEquipoId { get; set; }
    public virtual Equipo VisitanteEquipo { get; set; } = null!;
}

public class JornadaLibre : Jornada
{
    public required int EquipoLocalId { get; set; }
    public virtual Equipo EquipoLocal { get; set; } = null!;
}

public class JornadaInterzonal : Jornada
{
    public virtual Equipo Equipo { get; set; } = null!;
    public required int EquipoId { get; set; }

    [ForeignKey("LocalVisitante")]
    public required int LocalOVisitanteId { get; set; }
    public virtual LocalVisitante LocalVisitante { get; set; } = null!;
}