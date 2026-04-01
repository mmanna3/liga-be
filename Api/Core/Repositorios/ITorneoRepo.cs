using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoRepo : IRepositorioABM<Torneo>
{
    Task<Zona?> ObtenerZonaUnicaDeTorneo(int torneoId);
    Task CrearFaseUnicaYZonaUnica(int torneoId);
    Task<bool> ExisteTorneoConNombreAnioYAgrupador(string nombre, int anio, int torneoAgrupadorId, int? excluirId = null);
    Task<IEnumerable<Torneo>> ListarFiltrado(int? anio, int? torneoAgrupadorId);
} 