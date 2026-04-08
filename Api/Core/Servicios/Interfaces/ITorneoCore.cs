using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Servicios.Interfaces;

public interface ITorneoCore : ICoreABM<TorneoDTO>
{
    Task<IEnumerable<TorneoDTO>> Filtrar(int? anio, int? agrupadorId);

    Task CambiarVisibilidadEnApp(int id, bool esVisibleEnApp);

    Task EditarFasesParaTablaAnual(int id, EditarFasesParaTablaAnualDTO dto);
} 