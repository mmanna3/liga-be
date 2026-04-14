using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ConfiguracionCore : ABMCore<IConfiguracionRepo, Configuracion, ConfiguracionDTO>,
    IConfiguracionCore
{
    private readonly IRelojZonaHorariaArgentina _relojArgentina;
    private readonly IImagenEscudoRepo _imagenEscudoRepo;

    public ConfiguracionCore(
        IBDVirtual bd,
        IConfiguracionRepo repo,
        IMapper mapper,
        IRelojZonaHorariaArgentina relojArgentina,
        IImagenEscudoRepo imagenEscudoRepo) : base(bd, repo, mapper)
    {
        _relojArgentina = relojArgentina;
        _imagenEscudoRepo = imagenEscudoRepo;
    }

    public Task<bool> CambiarEscudoPorDefecto(CambiarEscudoPorDefectoDTO dto)
    {
        _imagenEscudoRepo.GuardarEscudoPorDefecto(dto.Escudo);
        return Task.FromResult(true);
    }

    public async Task<bool> FichajeEstaHabilitado()
    {
        var c = await Repo.ObtenerPorId(1);
        if (c is null)
            return false;

        return c.HabilitacionFichajeId switch
        {
            (int)HabilitacionFichajeEnum.Habilitado => true,
            (int)HabilitacionFichajeEnum.Deshabilitado => false,
            (int)HabilitacionFichajeEnum.Programado => FranjaHorariaFichajeProgramado.EstaActiva(
                _relojArgentina.AhoraLocal),
            _ => false,
        };
    }
}
