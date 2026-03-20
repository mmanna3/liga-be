using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class FixtureAlgoritmoCore : ABMCore<IFixtureAlgoritmoRepo, FixtureAlgoritmo, FixtureAlgoritmoDTO>, IFixtureAlgoritmoCore
{
    public FixtureAlgoritmoCore(IBDVirtual bd, IFixtureAlgoritmoRepo repo, IMapper mapper)
        : base(bd, repo, mapper)
    {
    }

    public override async Task<int> Crear(FixtureAlgoritmoDTO dto)
    {
        ValidarCantidadDeFechas(dto);
        ValidarEquiposUnaSolaVezPorFecha(dto);
        ValidarEncuentrosNoRepetidos(dto);
        var fixture = new FixtureAlgoritmo { Id = 0, CantidadDeEquipos = dto.CantidadDeEquipos, Nombre = dto.Nombre };
        var fechas = (dto.Fechas ?? []).Select(p => new FixtureAlgoritmoFecha
        {
            Id = 0,
            FixtureAlgoritmoId = 0,
            Fecha = p.Fecha,
            EquipoLocal = p.EquipoLocal,
            EquipoVisitante = p.EquipoVisitante,
            FixtureAlgoritmo = fixture
        }).ToList();
        fixture.Fechas = fechas;

        Repo.Crear(fixture);
        await BDVirtual.GuardarCambios();
        return fixture.Id;
    }

    protected override FixtureAlgoritmoDTO AntesDeObtenerPorId(FixtureAlgoritmo entidad, FixtureAlgoritmoDTO dto)
    {
        dto.FixtureAlgoritmoId = entidad.Id;
        return dto;
    }

    protected override async Task<FixtureAlgoritmo> AntesDeModificar(int id, FixtureAlgoritmoDTO dto, FixtureAlgoritmo entidadAnterior, FixtureAlgoritmo entidadNueva)
    {
        ValidarCantidadDeFechas(dto);
        ValidarEquiposUnaSolaVezPorFecha(dto);
        ValidarEncuentrosNoRepetidos(dto);
        await Repo.EliminarFechasDelFixture(id);

        var fechas = (dto.Fechas ?? []).Select(p => new FixtureAlgoritmoFecha
        {
            Id = 0,
            FixtureAlgoritmoId = id,
            Fecha = p.Fecha,
            EquipoLocal = p.EquipoLocal,
            EquipoVisitante = p.EquipoVisitante
        }).ToList();
        entidadNueva.Fechas = fechas;
        entidadNueva.Id = id;
        return entidadNueva;
    }

    /// <summary>
    /// Valida que la cantidad de fechas sea (N-1)*(N/2) para N equipos (round-robin).
    /// </summary>
    private static void ValidarCantidadDeFechas(FixtureAlgoritmoDTO dto)
    {
        var n = dto.CantidadDeEquipos;
        var cantidadEsperada = (n - 1) * (n / 2);
        var cantidadRecibida = (dto.Fechas ?? []).Count;
        if (cantidadRecibida != cantidadEsperada)
            throw new ExcepcionControlada(
                $"La cantidad de fechas debe ser {(n - 1)}×({n}/2) = {cantidadEsperada} para {n} equipos. Recibido: {cantidadRecibida}.");
    }

    /// <summary>
    /// Valida que en cada fecha (ronda) aparezcan todos los equipos exactamente una vez
    /// (como local o visitante). Los equipos se identifican del 1 a CantidadDeEquipos.
    /// </summary>
    private static void ValidarEquiposUnaSolaVezPorFecha(FixtureAlgoritmoDTO dto)
    {
        var n = dto.CantidadDeEquipos;
        var equiposEsperados = Enumerable.Range(1, n).ToHashSet();
        var fechas = dto.Fechas ?? [];

        foreach (var grupo in fechas.GroupBy(f => f.Fecha))
        {
            var equiposEnFecha = new List<int>();
            foreach (var p in grupo)
            {
                if (p.EquipoLocal < 1 || p.EquipoLocal > n)
                    throw new ExcepcionControlada(
                        $"En la fecha {grupo.Key}, el equipo local {p.EquipoLocal} está fuera de rango. Debe estar entre 1 y {n}.");
                if (p.EquipoVisitante < 1 || p.EquipoVisitante > n)
                    throw new ExcepcionControlada(
                        $"En la fecha {grupo.Key}, el equipo visitante {p.EquipoVisitante} está fuera de rango. Debe estar entre 1 y {n}.");
                equiposEnFecha.Add(p.EquipoLocal);
                equiposEnFecha.Add(p.EquipoVisitante);
            }

            var conjuntoEnFecha = equiposEnFecha.ToHashSet();
            if (conjuntoEnFecha.Count != equiposEnFecha.Count)
                throw new ExcepcionControlada(
                    $"En la fecha {grupo.Key} hay equipos repetidos. Cada equipo debe aparecer una sola vez por fecha.");

            if (!equiposEsperados.SetEquals(conjuntoEnFecha))
            {
                var faltan = equiposEsperados.Except(conjuntoEnFecha).ToList();
                var sobran = conjuntoEnFecha.Except(equiposEsperados).ToList();
                var mensaje = faltan.Count > 0
                    ? $"En la fecha {grupo.Key} faltan los equipos: {string.Join(", ", faltan)}."
                    : $"En la fecha {grupo.Key} aparecen equipos inválidos: {string.Join(", ", sobran)}. Deben ser del 1 al {n}.";
                throw new ExcepcionControlada(mensaje);
            }
        }
    }

    /// <summary>
    /// Valida que ningún par de equipos se repita como encuentro en todo el fixture.
    /// No importa quién sea local o visitante: (1,2) y (2,1) es el mismo encuentro.
    /// </summary>
    private static void ValidarEncuentrosNoRepetidos(FixtureAlgoritmoDTO dto)
    {
        var fechas = dto.Fechas ?? [];
        var encuentrosVistos = new HashSet<(int, int)>();

        foreach (var p in fechas)
        {
            var menor = Math.Min(p.EquipoLocal, p.EquipoVisitante);
            var mayor = Math.Max(p.EquipoLocal, p.EquipoVisitante);
            var par = (menor, mayor);

            if (!encuentrosVistos.Add(par))
                throw new ExcepcionControlada(
                    $"El encuentro entre equipo {menor} y equipo {mayor} está repetido. Cada par de equipos solo puede jugar una vez en todo el fixture.");
        }
    }
}
