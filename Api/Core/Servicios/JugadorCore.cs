using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class JugadorCore : ABMCore<IJugadorRepo, Jugador, JugadorDTO>, IJugadorCore
{
    public JugadorCore(IBDVirtual bd, IJugadorRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}