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
        var fixture = new FixtureAlgoritmo { Id = 0, CantidadDeEquipos = dto.CantidadDeEquipos };
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
}
