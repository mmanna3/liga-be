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

    /// <summary>Filas afectadas (0 si no existe el torneo).</summary>
    Task<int> ActualizarFasesParaTablaAnual(int torneoId, int? faseAperturaId, int? faseClausuraId);

    /// <summary>Anula referencias de tabla anual que apuntan a la fase (necesario con FK Restrict al borrar la fase).</summary>
    Task LimpiarReferenciasFaseTablaAnualSiCoinciden(int torneoId, int faseId);

    /// <summary>Torneo con categorías (sin fases/zonas), solo lectura.</summary>
    Task<Torneo?> ObtenerPorIdConCategoriasAsync(int id, CancellationToken cancellationToken = default);
} 