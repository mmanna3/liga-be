using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface ITorneoCore : ICoreABM<TorneoDTO>
{
    Task<IEnumerable<TorneoListadoDTO>> Listado(int? anio, int? agrupadorId);
    Task<IEnumerable<TorneoDTO>> Filtrar(int? anio, int? agrupadorId);

    Task CambiarVisibilidadEnApp(int id, bool esVisibleEnApp);

    Task EditarFasesParaTablaAnual(int id, EditarFasesParaTablaAnualDTO dto);

    /// <summary>Jugadores del torneo con tarjetas amarillas. Requiere que el agrupador lo permita (Futsal).</summary>
    Task<IEnumerable<JugadorConAmarillasDTO>> ListarJugadoresConAmarillas(int torneoId);

    /// <summary>Pone en cero las amarillas de todos los jugadores del torneo. Devuelve fichas afectadas.</summary>
    Task<int> LimpiarAmarillasDelTorneo(int torneoId);
}