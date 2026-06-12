using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class EstructuraFasesCore : IEstructuraFasesCore
{
    private readonly IBDVirtual _bdVirtual;
    private readonly ITorneoRepo _torneoRepo;
    private readonly IFaseRepo _faseRepo;
    private readonly IGrupoDeFasesRepo _grupoRepo;

    public EstructuraFasesCore(
        IBDVirtual bdVirtual,
        ITorneoRepo torneoRepo,
        IFaseRepo faseRepo,
        IGrupoDeFasesRepo grupoRepo)
    {
        _bdVirtual = bdVirtual;
        _torneoRepo = torneoRepo;
        _faseRepo = faseRepo;
        _grupoRepo = grupoRepo;
    }

    public async Task PersistirEstructura(int torneoId, EstructuraFasesDTO dto)
    {
        var torneo = await _torneoRepo.ObtenerPorId(torneoId);
        if (torneo == null)
            throw new ExcepcionControlada("El torneo indicado no existe.");

        var items = dto.Items ?? [];
        EstructuraFasesTreeBuilder.ValidarProfundidadEstructura(items);

        var faseIds = EstructuraFasesTreeBuilder.RecolectarFaseIds(items);
        var grupoIds = EstructuraFasesTreeBuilder.RecolectarGrupoIds(items);

        var fases = await _faseRepo.ListarPorPadreParaEditar(torneoId);
        var grupos = await _grupoRepo.ListarTodosPorTorneoParaEditar(torneoId);

        if (faseIds.Count != fases.Count)
            throw new ExcepcionControlada("La cantidad de fases no coincide con las del torneo.");

        if (grupoIds.Count != grupos.Count)
            throw new ExcepcionControlada("La cantidad de grupos no coincide con los del torneo.");

        var fasesExistentes = fases.Select(f => f.Id).ToHashSet();
        if (faseIds.Any(id => !fasesExistentes.Contains(id)))
            throw new ExcepcionControlada("Hay fases que no pertenecen al torneo indicado.");

        var gruposExistentes = grupos.Select(g => g.Id).ToHashSet();
        if (grupoIds.Any(id => !gruposExistentes.Contains(id)))
            throw new ExcepcionControlada("Hay grupos que no pertenecen al torneo indicado.");

        if (faseIds.Count != faseIds.Distinct().Count())
            throw new ExcepcionControlada("No puede haber fases repetidas en la estructura.");

        if (grupoIds.Count != grupoIds.Distinct().Count())
            throw new ExcepcionControlada("No puede haber grupos repetidos en la estructura.");

        foreach (var fase in fases)
            fase.Numero = -fase.Id;

        foreach (var grupo in grupos)
            grupo.Numero = -grupo.Id;

        await _bdVirtual.GuardarCambios();

        var fasesPorId = fases.ToDictionary(f => f.Id);
        var gruposPorId = grupos.ToDictionary(g => g.Id);

        AplicarEstructura(items, fasesPorId, gruposPorId, contenedorGrupoId: null);

        await _bdVirtual.GuardarCambios();
    }

    private static void AplicarEstructura(
        IReadOnlyList<EstructuraFasesItemDTO> items,
        Dictionary<int, Fase> fasesPorId,
        Dictionary<int, GrupoDeFases> gruposPorId,
        int? contenedorGrupoId)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var numero = i + 1;
            var item = items[i];

            if (item.Tipo == EstructuraFasesTreeBuilder.TipoFase)
            {
                if (item.FaseId is not > 0)
                    throw new ExcepcionControlada("Cada fase debe tener un identificador válido.");

                var fase = fasesPorId[item.FaseId.Value];
                fase.Numero = numero;
                fase.GrupoDeFasesId = contenedorGrupoId;
            }
            else if (item.Tipo == EstructuraFasesTreeBuilder.TipoGrupo)
            {
                if (item.GrupoId is not > 0)
                    throw new ExcepcionControlada("Cada grupo debe tener un identificador válido.");

                var grupo = gruposPorId[item.GrupoId.Value];
                grupo.Numero = numero;
                grupo.GrupoDeFasesPadreId = contenedorGrupoId;

                AplicarEstructura(item.Items ?? [], fasesPorId, gruposPorId, item.GrupoId.Value);
            }
            else
            {
                throw new ExcepcionControlada($"Tipo de elemento no válido: {item.Tipo}.");
            }
        }
    }
}
