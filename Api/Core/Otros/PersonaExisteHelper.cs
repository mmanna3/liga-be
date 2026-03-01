using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Otros;

/// <summary>
/// Determina si una persona "existe" en el sistema para los flujos de fichaje.
/// Solo se consideran existentes los que están aprobados.
/// </summary>
public static class PersonaExisteHelper
{
    private static readonly HashSet<int> EstadosJugadorExistentes =
    [
        (int)EstadoJugadorEnum.Activo,
        (int)EstadoJugadorEnum.Suspendido,
        (int)EstadoJugadorEnum.Inhabilitado,
        (int)EstadoJugadorEnum.AprobadoPendienteDePago
    ];

    /// <summary>
    /// Un delegado existe solo si tiene al menos un DelegadoClub Activo (aprobado).
    /// </summary>
    public static bool DelegadoExiste(Delegado? delegado) =>
        delegado != null && (delegado.DelegadoClubs ?? []).Any(dc => dc.EstadoDelegadoId == (int)EstadoDelegadoEnum.Activo);

    /// <summary>
    /// Un jugador existe si tiene al menos un JugadorEquipo con estado Activo, Suspendido, Inhabilitado o AprobadoPendienteDePago.
    /// </summary>
    public static bool JugadorExiste(Jugador? jugador) =>
        jugador != null && jugador.JugadorEquipos.Any(je => EstadosJugadorExistentes.Contains(je.EstadoJugadorId));

    /// <summary>
    /// Un delegado está pendiente si tiene al menos un DelegadoClub con estado PendienteDeAprobacion.
    /// </summary>
    public static bool DelegadoEstaPendiente(Delegado? delegado) =>
        delegado != null && (delegado.DelegadoClubs ?? []).Any(dc => dc.EstadoDelegadoId == (int)EstadoDelegadoEnum.PendienteDeAprobacion);

    /// <summary>
    /// Un jugador está pendiente si existe pero solo tiene JugadorEquipos con FichajePendienteDeAprobacion (ninguno aprobado).
    /// </summary>
    public static bool JugadorEstaPendiente(Jugador? jugador) =>
        jugador != null
        && jugador.JugadorEquipos.Count > 0
        && !JugadorExiste(jugador)
        && jugador.JugadorEquipos.Any(je => je.EstadoJugadorId == (int)EstadoJugadorEnum.FichajePendienteDeAprobacion);
}
