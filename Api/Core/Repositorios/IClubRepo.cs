using Api.Core.DTOs;
using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IClubRepo : IRepositorioABM<Club>
{
    /// <summary>Listado plano y liviano para el home de clubes (sin escudo ni colecciones).</summary>
    Task<List<ClubListadoDTO>> ListarParaListado();

    Task EliminarDelegadoClubsDelClub(int clubId);
    Task EliminarClubPorId(int clubId);
}