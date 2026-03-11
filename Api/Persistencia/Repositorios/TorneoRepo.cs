using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class TorneoRepo : RepositorioABM<Torneo>, ITorneoRepo
{
    public TorneoRepo(AppDbContext context) : base(context)
    {
    }
    
    protected override IQueryable<Torneo> Set()
    {
        return Context.Set<Torneo>()
            .Include(x => x.TorneoAgrupador)
            .Include(x => x.Fases)
                .ThenInclude(f => f.Zonas)
            .AsQueryable();
    }

    public async Task<TorneoZona?> ObtenerZonaUnicaDeTorneo(int torneoId)
    {
        return await Context.TorneoZonas
            .Include(tz => tz.TorneoFase)
            .Where(tz => tz.TorneoFase!.TorneoId == torneoId && tz.TorneoFase.Numero == 1 && tz.Nombre == "Zona única")
            .FirstOrDefaultAsync();
    }

    public async Task CrearFaseUnicaYZonaUnica(int torneoId)
    {
        var fase = new TorneoFase
        {
            Id = 0,
            TorneoId = torneoId,
            Numero = 1,
            FaseFormatoId = (int)FormatoDeLaFaseEnum.TodosContraTodos,
            EstadoFaseId = (int)EstadoFaseEnum.InicioPendiente,
            EsVisibleEnApp = true
        };
        Context.TorneoFases.Add(fase);
        await Context.SaveChangesAsync();

        var zona = new TorneoZona
        {
            Id = 0,
            TorneoFaseId = fase.Id,
            Nombre = "Zona única"
        };
        Context.TorneoZonas.Add(zona);
        await Context.SaveChangesAsync();
    }
} 