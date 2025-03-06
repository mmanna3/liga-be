using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Servicios.Interfaces;

public interface IJugadorCore : ICoreABM<JugadorDTO>
{
    Task<int> Gestionar(GestionarJugadorDTO dto);
    Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados);
}