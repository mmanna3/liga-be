using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class LeyendaTablaPosicionesCore
    : ABMCoreAnidado<ILeyendaTablaPosicionesRepo, LeyendaTablaPosiciones, LeyendaTablaPosicionesDTO, int>,
        ILeyendaTablaPosicionesCore
{
    private readonly IZonaRepo _zonaRepo;
    private readonly ITorneoCategoriaRepo _torneoCategoriaRepo;

    public LeyendaTablaPosicionesCore(
        IBDVirtual bd,
        ILeyendaTablaPosicionesRepo repo,
        IZonaRepo zonaRepo,
        ITorneoCategoriaRepo torneoCategoriaRepo,
        IMapper mapper)
        : base(bd, repo, mapper)
    {
        _zonaRepo = zonaRepo;
        _torneoCategoriaRepo = torneoCategoriaRepo;
    }

    protected override async Task<LeyendaTablaPosiciones> AntesDeCrear(int padreId, LeyendaTablaPosicionesDTO dto,
        LeyendaTablaPosiciones entidad)
    {
        await ValidarZonaYCategoria(padreId, dto.CategoriaId);
        if (await Repo.ExisteOtraConMismaZonaYCategoria(padreId, dto.CategoriaId, null))
            throw new ExcepcionControlada(
                "Ya existe una leyenda para esta zona y categoría (o ya hay una leyenda general sin categoría).");

        entidad.ZonaId = padreId;
        return entidad;
    }

    protected override async Task AntesDeModificar(int padreId, int id, LeyendaTablaPosicionesDTO dto,
        LeyendaTablaPosiciones entidadAnterior, LeyendaTablaPosiciones entidadNueva)
    {
        await ValidarZonaYCategoria(padreId, dto.CategoriaId);
        if (await Repo.ExisteOtraConMismaZonaYCategoria(padreId, dto.CategoriaId, id))
            throw new ExcepcionControlada(
                "Ya existe una leyenda para esta zona y categoría (o ya hay una leyenda general sin categoría).");

        entidadNueva.ZonaId = padreId;
    }

    private async Task ValidarZonaYCategoria(int zonaId, int? categoriaId)
    {
        var zona = await _zonaRepo.ObtenerPorId(zonaId);
        if (zona == null)
            throw new ExcepcionControlada("La zona no existe.");

        var torneoId = ObtenerTorneoIdDeZona(zona);
        if (!categoriaId.HasValue)
            return;

        var categoria = await _torneoCategoriaRepo.ObtenerPorId(categoriaId.Value);
        if (categoria == null)
            throw new ExcepcionControlada("La categoría no existe.");

        if (categoria.TorneoId != torneoId)
            throw new ExcepcionControlada("La categoría no pertenece al torneo de la zona.");
    }

    private static int ObtenerTorneoIdDeZona(Zona zona)
    {
        return zona switch
        {
            ZonaTodosContraTodos zt => zt.Fase.TorneoId,
            ZonaEliminacionDirecta ze => ze.Fase.TorneoId,
            _ => throw new ExcepcionControlada("Tipo de zona no soportado para leyendas.")
        };
    }
}
