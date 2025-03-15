using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IEquipoRepo : IRepositorioABM<Equipo>
{
    Task<bool> ExisteEquipoConMismoNombreEnTorneo(string nombre, int? torneoId, int? equipoIdExcluir = null);
}