using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class SponsorWebPublicaCore : ABMCore<ISponsorWebPublicaRepo, SponsorWebPublica, SponsorWebPublicaDTO>,
    ISponsorWebPublicaCore
{
    private readonly IImagenSponsorWebPublicaRepo _imagenSponsorWebPublicaRepo;

    public SponsorWebPublicaCore(
        IBDVirtual bd,
        ISponsorWebPublicaRepo repo,
        IMapper mapper,
        IImagenSponsorWebPublicaRepo imagenSponsorWebPublicaRepo)
        : base(bd, repo, mapper)
    {
        _imagenSponsorWebPublicaRepo = imagenSponsorWebPublicaRepo;
    }

    public override async Task<IEnumerable<SponsorWebPublicaDTO>> Listar()
    {
        var dtos = (await base.Listar()).ToList();
        foreach (var dto in dtos)
            dto.Imagen = ImagenUtility.AgregarMimeType(_imagenSponsorWebPublicaRepo.GetImagenEnBase64(dto.Id));
        return dtos;
    }

    protected override SponsorWebPublicaDTO AntesDeObtenerPorId(SponsorWebPublica entidad, SponsorWebPublicaDTO dto)
    {
        dto.Imagen = ImagenUtility.AgregarMimeType(_imagenSponsorWebPublicaRepo.GetImagenEnBase64(entidad.Id));
        return dto;
    }

    public async Task<int> CrearConImagen(CrearSponsorWebPublicaDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ExcepcionControlada("El nombre del sponsor es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.ImagenBase64))
            throw new ExcepcionControlada("La imagen del sponsor es obligatoria.");

        var entidad = new SponsorWebPublica
        {
            Id = 0,
            Nombre = dto.Nombre.Trim(),
            Orden = await Repo.ObtenerProximoOrdenAsync()
        };

        Repo.Crear(entidad);
        await BDVirtual.GuardarCambios();
        _imagenSponsorWebPublicaRepo.Guardar(entidad.Id, dto.ImagenBase64);
        return entidad.Id;
    }

    protected override Task AntesDeEliminar(int id, SponsorWebPublica entidad)
    {
        _imagenSponsorWebPublicaRepo.Eliminar(id);
        return Task.CompletedTask;
    }
}
