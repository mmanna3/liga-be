using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class RelojZonaHorariaArgentina : IRelojZonaHorariaArgentina
{
    private static readonly TimeZoneInfo Zona = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "Argentina Standard Time" : "America/Argentina/Buenos_Aires");

    public DateTime AhoraLocal => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zona);
}
