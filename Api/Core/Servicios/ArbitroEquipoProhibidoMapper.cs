using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;

namespace Api.Core.Servicios;

public static class ArbitroEquipoProhibidoMapper
{
    public static ArbitroEquipoProhibidoDTO Map(ArbitroEquipoProhibido src)
    {
        var equipo = src.Equipo;
        return new ArbitroEquipoProhibidoDTO
        {
            EquipoId = src.EquipoId,
            Nombre = equipo.Nombre,
            ClubNombre = equipo.Club?.Nombre,
            CodigoAlfanumerico = ObtenerCodigoAlfanumerico(equipo.Id),
            TorneosActuales = ObtenerTorneosActuales(equipo)
        };
    }

    private static string ObtenerCodigoAlfanumerico(int equipoId) =>
        equipoId > 0 && equipoId < 10000
            ? GeneradorDeHash.GenerarAlfanumerico7Digitos(equipoId)
            : string.Empty;

    public static List<string> ObtenerTorneosActuales(Equipo equipo)
    {
        var anio = DateTime.Today.Year;
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var ez in equipo.Zonas ?? [])
        {
            if (ez.Zona == null) continue;

            Fase? fase = ez.Zona switch
            {
                ZonaTodosContraTodos z => z.Fase,
                ZonaEliminacionDirecta z => z.Fase,
                _ => null
            };

            var torneo = fase?.Torneo;
            if (torneo?.Anio == anio && !string.IsNullOrWhiteSpace(torneo.Nombre))
                nombres.Add($"{torneo.Nombre} {torneo.Anio}");
        }

        return nombres.OrderBy(x => x).ToList();
    }
}
