using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ClubRepo : RepositorioABM<Club>, IClubRepo
{
    public ClubRepo(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Listado liviano para el home de clubes: proyección plana con
    /// <c>AsNoTracking</c>, sin el escudo en base64 ni las colecciones de
    /// equipos / delegados.
    /// </summary>
    public async Task<List<ClubListadoDTO>> ListarParaListado()
    {
        return await Context.Set<Club>()
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .Select(c => new ClubListadoDTO
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Localidad = c.Localidad,
                CanchaTipoId = c.CanchaTipoId,
                // El .Tipo crudo; el Core lo formatea (Pasto Natural, etc.).
                CanchaTipo = c.CanchaTipo.Tipo
            })
            .ToListAsync();
    }

    protected override IQueryable<Club> Set()
    {
        return Context.Set<Club>()
            .Include(x => x.CanchaTipo)
            .Include(x => x.DelegadoClubs)
                .ThenInclude(dc => dc.Delegado)
            .Include(x => x.DelegadoClubs)
                .ThenInclude(dc => dc.EstadoDelegado)
            .Include("Equipos.Zonas.Zona.Fase.Torneo")
            .AsSplitQuery()
            .AsQueryable();
    }

    public override void Eliminar(Club club)
    {
        Context.Clubs.Remove(club);
    }

    public async Task EliminarClubPorId(int clubId)
    {
        await Context.Clubs.Where(c => c.Id == clubId).ExecuteDeleteAsync();
    }

    public async Task EliminarDelegadoClubsDelClub(int clubId)
    {
        await Context.DelegadoClub
            .Where(dc => dc.ClubId == clubId)
            .ExecuteDeleteAsync();
    }
}