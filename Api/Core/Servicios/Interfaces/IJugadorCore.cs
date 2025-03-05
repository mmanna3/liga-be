using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface IJugadorCore : ICoreABM<JugadorDTO>
{
    Task<int> Gestionar(GestionarJugadorDTO dto);
}