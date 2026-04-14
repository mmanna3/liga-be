namespace Api.Core.Servicios;

/// <summary>
/// Regla "Programado": entre lunes 8:00 y jueves 20:00 (hora civil Argentina).
/// </summary>
internal static class FranjaHorariaFichajeProgramado
{
    private static readonly TimeSpan HoraInicio = new(8, 0, 0);
    private static readonly TimeSpan HoraFin = new(20, 0, 0);

    internal static bool EstaActiva(DateTime horaLocalArgentina)
    {
        return horaLocalArgentina.DayOfWeek switch
        {
            DayOfWeek.Monday => horaLocalArgentina.TimeOfDay >= HoraInicio,
            DayOfWeek.Tuesday or DayOfWeek.Wednesday => true,
            DayOfWeek.Thursday => horaLocalArgentina.TimeOfDay <= HoraFin,
            _ => false
        };
    }
}
