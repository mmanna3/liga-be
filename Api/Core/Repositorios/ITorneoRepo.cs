using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoRepo : IRepositorioABM<Torneo>
{
    /// <summary>Listado plano y liviano (home de torneos + selectores).</summary>
    Task<List<TorneoListadoDTO>> ListarParaListado(int? anio, int? torneoAgrupadorId);
    Task<Zona?> ObtenerZonaUnicaDeTorneo(int torneoId);
    Task CrearFaseUnicaYZonaUnica(int torneoId);
    Task<bool> ExisteTorneoConNombreAnioYAgrupador(string nombre, int anio, int torneoAgrupadorId, int? excluirId = null);
    Task<IEnumerable<Torneo>> ListarFiltrado(int? anio, int? torneoAgrupadorId);

    /// <summary>Filas afectadas (0 si no existe el torneo).</summary>
    Task<int> ActualizarEsVisibleEnApp(int id, bool esVisibleEnApp);

    /// <summary>Filas afectadas (0 si no existe el torneo).</summary>
    Task<int> ActualizarFasesParaTablaAnual(int torneoId, int? faseAperturaId, int? faseClausuraId);

    /// <summary>Anula referencias de tabla anual que apuntan a la fase (necesario con FK Restrict al borrar la fase).</summary>
    Task LimpiarReferenciasFaseTablaAnualSiCoinciden(int torneoId, int faseId);

    /// <summary>Torneo con categorías (sin fases/zonas), solo lectura.</summary>
    Task<Torneo?> ObtenerPorIdConCategoriasAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// True/False si el agrupador del torneo habilita limpiar amarillas; null si el torneo no existe.
    /// </summary>
    Task<bool?> AgrupadorPermiteLimpiarAmarillas(int torneoId);

    /// <summary>
    /// Jugadores del torneo (por sus equipos en cualquier zona/fase) con al menos una tarjeta amarilla.
    /// Ordenado por club, equipo y apellido.
    /// </summary>
    Task<List<JugadorConAmarillasDTO>> ListarJugadoresConAmarillasPorTorneo(int torneoId);

    /// <summary>
    /// Pone en cero las tarjetas amarillas de todas las fichas de los equipos del torneo.
    /// Las rojas no se tocan. Devuelve la cantidad de fichas afectadas.
    /// </summary>
    Task<int> LimpiarAmarillasDelTorneo(int torneoId);
}