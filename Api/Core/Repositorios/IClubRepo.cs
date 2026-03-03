using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IClubRepo : IRepositorioABM<Club>
{
    void Eliminar(Club club);
}