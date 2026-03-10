using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface ITorneoRepo : IRepositorioABM<Torneo>
{
    Task<TorneoZona?> ObtenerZonaUnicaDeTorneo(int torneoId);
    Task CrearFaseUnicaYZonaUnica(int torneoId);
} 