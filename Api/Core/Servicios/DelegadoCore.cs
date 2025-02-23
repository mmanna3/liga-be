using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class DelegadoCore : ABMCore<IDelegadoRepo, Delegado, DelegadoDTO>, IDelegadoCore
{
    public DelegadoCore(IBDVirtual bd, IDelegadoRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}