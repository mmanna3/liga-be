using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ConfiguracionCore : ABMCore<IConfiguracionRepo, Configuracion, ConfiguracionDTO>,
    IConfiguracionCore
{
    public ConfiguracionCore(IBDVirtual bd, IConfiguracionRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }

    public async Task<bool> FichajeEstaHabilitado()
    {
        var c = await Repo.ObtenerPorId(1);
        return c?.FichajeEstaHabilitado ?? false;
    }
}
