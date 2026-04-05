using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IEquipoRepo : IRepositorioABM<Equipo>
{
    Task<bool> ExisteEquipoConMismoNombreEnZona(string nombre, IEnumerable<int> zonaIds, int? equipoIdExcluir = null);
    Task<int> ContarEquiposDelJugador(int jugadorId);
    Task QuitarEquiposDeZona(int zonaId);
    Task AsignarEquiposAZona(int zonaId, IEnumerable<int> equipoIds);
    Task SincronizarZonasDelEquipo(int equipoId, IEnumerable<int> zonaIds);
    Task<IEnumerable<Equipo>> ListarConZonasParaEquiposParaZonas();

    /// <summary>Equipos asignados a la zona, con club, ordenados por nombre de equipo.</summary>
    Task<IReadOnlyList<Equipo>> ListarPorZonaIdAsync(int zonaId, CancellationToken cancellationToken = default);
}