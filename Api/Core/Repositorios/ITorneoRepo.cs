using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoRepo : IRepositorioABM<Torneo>
{
    Task<Zona?> ObtenerZonaUnicaDeTorneo(int torneoId);
    Task CrearFaseUnicaYZonaUnica(int torneoId);
    Task<bool> ExisteTorneoConNombreAnioYAgrupador(string nombre, int anio, int torneoAgrupadorId, int? excluirId = null);
    Task<IEnumerable<Torneo>> ListarFiltrado(int? anio, int? torneoAgrupadorId);

    /// <summary>Filas afectadas (0 si no existe el torneo).</summary>
    Task<int> ActualizarEsVisibleEnApp(int id, bool esVisibleEnApp);

    /// <summary>Torneo con categorías (sin fases/zonas), solo lectura.</summary>
    Task<Torneo?> ObtenerPorIdConCategoriasAsync(int id, CancellationToken cancellationToken = default);
} 