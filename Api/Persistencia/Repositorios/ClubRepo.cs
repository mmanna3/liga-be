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
    
    protected override IQueryable<Club> Set()
    {
        return Context.Set<Club>()
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