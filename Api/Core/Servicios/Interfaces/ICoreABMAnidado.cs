using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

/// <summary>
/// Interfaz para entidades que siempre dependen de un padre (sub-recursos).
/// </summary>
public interface ICoreABMAnidado<TPadreId, TDTO>
    where TDTO : DTO
{
    Task<IEnumerable<TDTO>> ListarPorPadre(TPadreId padreId);
    Task<int> Crear(TPadreId padreId, TDTO dto);
    Task<TDTO?> ObtenerPorId(TPadreId padreId, int id);
    Task<int> Modificar(TPadreId padreId, int id, TDTO nuevo);
    Task<int> Eliminar(TPadreId padreId, int id);
}
