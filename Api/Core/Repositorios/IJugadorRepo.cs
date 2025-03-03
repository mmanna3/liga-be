using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IJugadorRepo : IRepositorioABM<Jugador>
{
    Task<Jugador?> ObtenerPorDNI(string dni);
    void SiElDNISeHabiaFichadoYEstaRechazadoEliminarJugador(string entidadDNI);
}