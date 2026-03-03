using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ClubCore : ABMCore<IClubRepo, Club, ClubDTO>, IClubCore
{
    private readonly IImagenEscudoRepo _imagenEscudoRepo;

    public ClubCore(IBDVirtual bd, IClubRepo repo, IMapper mapper, IImagenEscudoRepo imagenEscudoRepo)
        : base(bd, repo, mapper)
    {
        _imagenEscudoRepo = imagenEscudoRepo;
    }

    public override async Task<IEnumerable<ClubDTO>> Listar()
    {
        var dtos = await base.Listar();
        foreach (var dto in dtos)
            dto.Escudo = _imagenEscudoRepo.PathRelativo(dto.Id);
        return dtos;
    }

    protected override ClubDTO AntesDeObtenerPorId(Club entidad, ClubDTO dto)
    {
        dto.Escudo = _imagenEscudoRepo.PathRelativo(entidad.Id);
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

    public async Task<int> Eliminar(int id)
    {
        var club = await Repo.ObtenerPorId(id);
        if (club == null)
            return -1;

        _imagenEscudoRepo.Eliminar(id);
        Repo.Eliminar(club);
        await BDVirtual.GuardarCambios();
        return id;
    }
}