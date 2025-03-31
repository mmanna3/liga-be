using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
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
        
        if (jugador != null)
        {
            var elJugadorEstaRechazadoEnAlgunEquipo = jugador.JugadorEquipos.Any(x => x.EstadoJugador.Id == (int)EstadoJugadorEnum.FichajeRechazado);
            return !elJugadorEstaRechazadoEnAlgunEquipo;
        }

        return false;
    }

}