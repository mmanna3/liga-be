using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ArbitroCore : ABMCore<IArbitroRepo, Arbitro, ArbitroDTO>, IArbitroCore
{
    public ArbitroCore(IBDVirtual bd, IArbitroRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}
