using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class PublicoCore : IPublicoCore
{
    private readonly IJugadorRepo _jugadorRepo;

    public PublicoCore(IJugadorRepo jugadorRepo)
    {
        _jugadorRepo = jugadorRepo;
    }

    public async Task<bool> ElDniEstaFichado(string dni)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dni);
        return jugador != null;
    }
}