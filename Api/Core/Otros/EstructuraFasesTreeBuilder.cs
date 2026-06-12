using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;

namespace Api.Core.Otros;

public static class EstructuraFasesTreeBuilder
{
    public const string TipoFase = "fase";
    public const string TipoGrupo = "grupo";

    public static List<EstructuraFasesItemDTO> ConstruirElementosDesdeEntidades(
        IEnumerable<Fase> fases,
        IEnumerable<GrupoDeFases> grupos,
        int? contenedorGrupoId = null)
    {
        var fasesList = fases.ToList();
        var gruposList = grupos.ToList();
        var items = new List<(int Numero, int Id, EstructuraFasesItemDTO Item)>();

        foreach (var f in fasesList.Where(f => f.GrupoDeFasesId == contenedorGrupoId))
        {
            items.Add((f.Numero, f.Id, new EstructuraFasesItemDTO
            {
                Tipo = TipoFase,
                FaseId = f.Id
            }));
        }

        foreach (var g in gruposList.Where(g => g.GrupoDeFasesPadreId == contenedorGrupoId))
        {
            items.Add((g.Numero, g.Id, new EstructuraFasesItemDTO
            {
                Tipo = TipoGrupo,
                GrupoId = g.Id,
                Items = ConstruirElementosDesdeEntidades(fasesList, gruposList, g.Id)
            }));
        }

        return items
            .OrderBy(x => x.Numero)
            .ThenBy(x => x.Id)
            .Select(x => x.Item)
            .ToList();
    }

    public static List<InformacionInicialElementoTorneoDTO> ConstruirElementosInformacionInicial(
        IEnumerable<Fase> fases,
        IEnumerable<GrupoDeFases> grupos,
        Dictionary<int, List<InformacionInicialZonaDTO>> zonasPorFaseId,
        int? contenedorGrupoId = null)
    {
        var fasesList = fases.ToList();
        var gruposList = grupos.ToList();
        var items = new List<(int Numero, int Id, InformacionInicialElementoTorneoDTO Item)>();

        foreach (var f in fasesList.Where(f => f.GrupoDeFasesId == contenedorGrupoId))
        {
            items.Add((f.Numero, f.Id, new InformacionInicialElementoTorneoDTO
            {
                Tipo = TipoFase,
                Id = f.Id,
                Nombre = f.Nombre,
                TipoDeFase = f switch
                {
                    FaseTodosContraTodos => nameof(TipoDeFaseEnum.TodosContraTodos),
                    FaseEliminacionDirecta => nameof(TipoDeFaseEnum.EliminacionDirecta),
                    _ => string.Empty
                },
                Zonas = zonasPorFaseId.GetValueOrDefault(f.Id) ?? []
            }));
        }

        foreach (var g in gruposList.Where(g => g.GrupoDeFasesPadreId == contenedorGrupoId))
        {
            items.Add((g.Numero, g.Id, new InformacionInicialElementoTorneoDTO
            {
                Tipo = TipoGrupo,
                GrupoId = g.Id,
                NombreGrupo = g.Nombre,
                Elementos = ConstruirElementosInformacionInicial(fasesList, gruposList, zonasPorFaseId, g.Id)
            }));
        }

        return items
            .OrderBy(x => x.Numero)
            .ThenBy(x => x.Id)
            .Select(x => x.Item)
            .ToList();
    }

    public static void ValidarProfundidadEstructura(IReadOnlyList<EstructuraFasesItemDTO> items, int profundidadGrupo = 0)
    {
        foreach (var item in items)
        {
            if (item.Tipo == TipoFase)
            {
                if (item.FaseId is not > 0)
                    throw new ExcepcionControlada("Cada fase debe tener un identificador válido.");
                if (item.Items is { Count: > 0 })
                    throw new ExcepcionControlada("Una fase no puede contener elementos hijos.");
                continue;
            }

            if (item.Tipo != TipoGrupo)
                throw new ExcepcionControlada($"Tipo de elemento no válido: {item.Tipo}.");

            if (item.GrupoId is not > 0)
                throw new ExcepcionControlada("Cada grupo debe tener un identificador válido.");

            var subItems = item.Items ?? [];
            var nuevaProfundidad = profundidadGrupo + 1;
            if (nuevaProfundidad > 2)
                throw new ExcepcionControlada("No se permiten más de dos niveles de grupos de fases.");

            foreach (var sub in subItems)
            {
                if (sub.Tipo == TipoGrupo && nuevaProfundidad >= 2)
                    throw new ExcepcionControlada("Un subgrupo no puede contener otro grupo de fases.");
            }

            ValidarProfundidadEstructura(subItems, nuevaProfundidad);
        }
    }

    public static HashSet<int> RecolectarFaseIds(IReadOnlyList<EstructuraFasesItemDTO> items)
    {
        var ids = new HashSet<int>();
        RecolectarFaseIdsRecursivo(items, ids);
        return ids;
    }

    public static HashSet<int> RecolectarGrupoIds(IReadOnlyList<EstructuraFasesItemDTO> items)
    {
        var ids = new HashSet<int>();
        RecolectarGrupoIdsRecursivo(items, ids);
        return ids;
    }

    private static void RecolectarFaseIdsRecursivo(IReadOnlyList<EstructuraFasesItemDTO> items, HashSet<int> ids)
    {
        foreach (var item in items)
        {
            if (item.Tipo == TipoFase)
            {
                if (item.FaseId is > 0)
                    ids.Add(item.FaseId.Value);
            }
            else if (item.Items != null)
            {
                RecolectarFaseIdsRecursivo(item.Items, ids);
            }
        }
    }

    private static void RecolectarGrupoIdsRecursivo(IReadOnlyList<EstructuraFasesItemDTO> items, HashSet<int> ids)
    {
        foreach (var item in items)
        {
            if (item.Tipo == TipoGrupo)
            {
                if (item.GrupoId is > 0)
                    ids.Add(item.GrupoId.Value);
                if (item.Items != null)
                    RecolectarGrupoIdsRecursivo(item.Items, ids);
            }
        }
    }
}
