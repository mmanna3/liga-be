using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IEquipoRepo : IRepositorioABM<Equipo>
{
    Task<bool> ExisteEquipoConMismoNombreEnZona(string nombre, int? zonaActualId, int? equipoIdExcluir = null);
    Task<int> ContarEquiposDelJugador(int jugadorId);
}