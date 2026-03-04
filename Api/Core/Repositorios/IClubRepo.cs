using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IClubRepo : IRepositorioABM<Club>
{
    Task EliminarDelegadoClubsDelClub(int clubId);
    Task EliminarClubPorId(int clubId);
}