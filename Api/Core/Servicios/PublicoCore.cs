using Api.Core.DTOs;
using Api.Core.Entidades;
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
    private readonly IDelegadoRepo _delegadoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;
    private readonly IBDVirtual _bdVirtual;

    public PublicoCore(IJugadorRepo jugadorRepo, IJugadorCore jugadorCore, IDelegadoRepo delegadoRepo, IImagenJugadorRepo imagenJugadorRepo, IBDVirtual bdVirtual)
    {
        _jugadorRepo = jugadorRepo;
        _jugadorCore = jugadorCore;
        _delegadoRepo = delegadoRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
        _bdVirtual = bdVirtual;
    }

    public async Task<bool> ElDniEstaFichado(string dni)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dni);
        var delegado = await _delegadoRepo.ObtenerPorDNI(dni);

        return PersonaExisteHelper.JugadorExiste(jugador) || PersonaExisteHelper.DelegadoExiste(delegado);
    }

    public async Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dto.DNI);
        if (PersonaExisteHelper.JugadorEstaPendiente(jugador))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como jugador. La administración debe aprobarlo antes de poder fichar en otro equipo.");

        var delegado = await _delegadoRepo.ObtenerPorDNI(dto.DNI);
        if (PersonaExisteHelper.DelegadoEstaPendiente(delegado))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como delegado. La administración debe aprobarlo antes de poder fichar en otro equipo.");

        if (PersonaExisteHelper.JugadorExiste(jugador))
        {
            var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
            await _jugadorCore.FicharJugadorEnElEquipo(equipoId, jugador!);
            await _bdVirtual.GuardarCambios();
            return jugador!.Id;
        }

        if (PersonaExisteHelper.DelegadoExiste(delegado))
        {
            return await FicharJugadorDesdeDelegado(delegado!, dto.CodigoAlfanumerico);
        }

        throw new ExcepcionControlada("No existe ni un jugador ni un delegado con el DNI indicado.");
    }

    private async Task<int> FicharJugadorDesdeDelegado(Delegado delegado, string codigoAlfanumerico)
    {
        _imagenJugadorRepo.CopiarFotosDeDelegadoATemporales(delegado.DNI);

        var jugador = new Jugador
        {
            Id = 0,
            DNI = delegado.DNI,
            Nombre = delegado.Nombre,
            Apellido = delegado.Apellido,
            FechaNacimiento = delegado.FechaNacimiento
        };

        var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        await _jugadorCore.FicharJugadorEnElEquipo(equipoId, jugador);

        _jugadorRepo.Crear(jugador);
        await _bdVirtual.GuardarCambios();

        return jugador.Id;
    }
}