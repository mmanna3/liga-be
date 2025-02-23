using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class EquipoCore : ABMCore<IEquipoRepo, Equipo, EquipoDTO>, IEquipoCore
{
    public EquipoCore(IBDVirtual bd, IEquipoRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}