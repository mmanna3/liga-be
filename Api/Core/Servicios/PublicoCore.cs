using Api.Core.DTOs;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class PublicoCore : IPublicoCore
{
    private readonly IJugadorRepo _jugadorRepo;
    private readonly IJugadorCore _jugadorCore;
    private readonly IBDVirtual _bdVirtual;

    public PublicoCore(IJugadorRepo jugadorRepo, IJugadorCore jugadorCore, IBDVirtual bdVirtual)
    {
        _jugadorRepo = jugadorRepo;
        _jugadorCore = jugadorCore;
        _bdVirtual = bdVirtual;
    }

    public async Task<bool> ElDniEstaFichado(string dni)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dni);
        
        if (jugador != null)
        {
            var elJugadorNoEstaAprobado = jugador.JugadorEquipos.Any(x => x.EstadoJugador.Id is (int)EstadoJugadorEnum.FichajeRechazado or (int)EstadoJugadorEnum.FichajePendienteDeAprobacion);
            return !elJugadorNoEstaAprobado;
        }

        return false;
    }

    public async Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dto.DNI);
        
        if (jugador == null)
            throw new ExcepcionControlada("El jugador no existe.");
            
        var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
        await _jugadorCore.FicharJugadorEnElEquipo(equipoId, jugador);

        await _bdVirtual.GuardarCambios();

        return jugador.Id;
    }
}