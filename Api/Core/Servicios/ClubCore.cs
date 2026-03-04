using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ClubCore : ABMCore<IClubRepo, Club, ClubDTO>, IClubCore
{
    private readonly IImagenEscudoRepo _imagenEscudoRepo;
    private readonly IEquipoCore _equipoCore;
    private readonly IDelegadoRepo _delegadoRepo;

    public ClubCore(IBDVirtual bd, IClubRepo repo, IMapper mapper, IImagenEscudoRepo imagenEscudoRepo, IEquipoCore equipoCore, IDelegadoRepo delegadoRepo)
        : base(bd, repo, mapper)
    {
        _imagenEscudoRepo = imagenEscudoRepo;
        _equipoCore = equipoCore;
        _delegadoRepo = delegadoRepo;
    }

    public override async Task<IEnumerable<ClubDTO>> Listar()
    {
        var dtos = await base.Listar();
        foreach (var dto in dtos)
            dto.Escudo = ImagenUtility.AgregarMimeType(_imagenEscudoRepo.GetEscudoEnBase64(dto.Id));
        return dtos;
    }

    protected override ClubDTO AntesDeObtenerPorId(Club entidad, ClubDTO dto)
    {
        dto.Escudo = ImagenUtility.AgregarMimeType(_imagenEscudoRepo.GetEscudoEnBase64(entidad.Id));
        return dto;
    }

    public async Task<int> CambiarEscudo(int clubId, string imagenBase64)
    {
        var club = await Repo.ObtenerPorId(clubId);
        if (club == null)
            throw new ExcepcionControlada("No existe el club indicado");

        _imagenEscudoRepo.Guardar(clubId, imagenBase64);
        return clubId;
    }

    public override async Task<int> Eliminar(int id)
    {
        var entidad = await Repo.ObtenerPorId(id);
        if (entidad == null)
            return -1;

        await AntesDeEliminar(id, entidad);
        await Repo.EliminarClubPorId(id);
        await BDVirtual.GuardarCambios();
        return id;
    }

    protected override async Task AntesDeEliminar(int id, Club entidad)
    {
        foreach (var equipo in entidad.Equipos)
            await _equipoCore.Eliminar(equipo.Id);

        var delegadosASoloEnEsteClub = new List<int>();
        foreach (var dc in entidad.DelegadoClubs)
        {
            var cantidadClubs = await _delegadoRepo.ContarClubsDelDelegado(dc.DelegadoId);
            if (cantidadClubs == 1)
                delegadosASoloEnEsteClub.Add(dc.DelegadoId);
        }

        await Repo.EliminarDelegadoClubsDelClub(id);

        foreach (var delegadoId in delegadosASoloEnEsteClub)
        {
            var delegado = await _delegadoRepo.ObtenerPorId(delegadoId);
            if (delegado != null)
                _delegadoRepo.Eliminar(delegado);
        }

        _imagenEscudoRepo.Eliminar(id);
        await Task.CompletedTask;
    }
}