using Api.Core.Entidades;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.TestsUtilidades;

public static class CategoriasDePrueba
{
    public static async Task<int> ObtenerFaseIdDeZona(AppDbContext context, int zonaId) =>
        await context.Zonas.Where(z => z.Id == zonaId).Select(z => z.FaseId).FirstAsync();

    public static async Task<FaseCategoria> AgregarFaseCategoria(
        AppDbContext context,
        int faseId,
        string nombre,
        int orden,
        int anioDesde = 2010,
        int anioHasta = 2020)
    {
        var cat = new FaseCategoria
        {
            Id = 0,
            Nombre = nombre,
            AnioDesde = anioDesde,
            AnioHasta = anioHasta,
            FaseId = faseId,
            Orden = orden
        };
        context.FaseCategorias.Add(cat);
        await context.SaveChangesAsync();
        return cat;
    }

    public static async Task<List<FaseCategoria>> AgregarFaseCategorias(
        AppDbContext context,
        int faseId,
        int cantidad,
        string nombrePrefix = "Cat Test")
    {
        var maxOrden = await context.FaseCategorias
            .Where(c => c.FaseId == faseId)
            .Select(c => (int?)c.Orden)
            .MaxAsync();
        var baseOrden = maxOrden ?? 0;
        var resultado = new List<FaseCategoria>();
        for (var i = 0; i < cantidad; i++)
        {
            resultado.Add(await AgregarFaseCategoria(
                context,
                faseId,
                $"{nombrePrefix} {baseOrden + i + 1}",
                baseOrden + i + 1));
        }

        return resultado;
    }

    public static async Task AgregarSoloPlantillaTorneo(
        AppDbContext context,
        int torneoId,
        int cantidad,
        string nombrePrefix = "Cat Test")
    {
        var maxOrden = await context.TorneoCategorias
            .Where(c => c.TorneoId == torneoId)
            .Select(c => (int?)c.Orden)
            .MaxAsync();
        var baseOrden = maxOrden ?? 0;

        for (var i = 0; i < cantidad; i++)
        {
            context.TorneoCategorias.Add(new TorneoCategoria
            {
                Id = 0,
                Nombre = $"{nombrePrefix} {baseOrden + i + 1}",
                AnioDesde = 2010,
                AnioHasta = 2020,
                TorneoId = torneoId,
                Orden = baseOrden + i + 1
            });
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Crea categorías plantilla del torneo y réplicas operativas en todas sus fases.
    /// </summary>
    public static async Task<List<FaseCategoria>> AgregarTorneoPlantillaYReplicarEnFases(
        AppDbContext context,
        int torneoId,
        int cantidad,
        string nombrePrefix = "Cat Test")
    {
        var maxOrden = await context.TorneoCategorias
            .Where(c => c.TorneoId == torneoId)
            .Select(c => (int?)c.Orden)
            .MaxAsync();
        var baseOrden = maxOrden ?? 0;

        var plantillas = new List<TorneoCategoria>();
        for (var i = 0; i < cantidad; i++)
        {
            plantillas.Add(new TorneoCategoria
            {
                Id = 0,
                Nombre = $"{nombrePrefix} {baseOrden + i + 1}",
                AnioDesde = 2010,
                AnioHasta = 2020,
                TorneoId = torneoId,
                Orden = baseOrden + i + 1
            });
        }

        context.TorneoCategorias.AddRange(plantillas);
        await context.SaveChangesAsync();

        var faseIds = await context.Fases.Where(f => f.TorneoId == torneoId).Select(f => f.Id).ToListAsync();
        var faseCategorias = new List<FaseCategoria>();
        foreach (var faseId in faseIds)
        {
            foreach (var plantilla in plantillas)
            {
                faseCategorias.Add(await AgregarFaseCategoria(
                    context,
                    faseId,
                    plantilla.Nombre,
                    plantilla.Orden,
                    plantilla.AnioDesde,
                    plantilla.AnioHasta));
            }
        }

        return faseCategorias;
    }
}
