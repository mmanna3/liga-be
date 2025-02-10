using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class ClubCore : ABMCore<IClubRepo, Club, ClubDTO>, IClubCore
{
    public ClubCore(IBDVirtual bd, IClubRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}