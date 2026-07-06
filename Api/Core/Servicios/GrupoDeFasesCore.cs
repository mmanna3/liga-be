using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class GrupoDeFasesCore : ABMCoreAnidado<IGrupoDeFasesRepo, GrupoDeFases, GrupoDeFasesDTO, int>, IGrupoDeFasesCore
{
    private readonly ITorneoRepo _torneoRepo;
    private readonly IFaseRepo _faseRepo;

    public GrupoDeFasesCore(
        IBDVirtual bd,
        IGrupoDeFasesRepo repo,
        ITorneoRepo torneoRepo,
        IFaseRepo faseRepo,
        IMapper mapper)
        : base(bd, repo, mapper)
    {
        _torneoRepo = torneoRepo;
        _faseRepo = faseRepo;
    }

    protected override async Task<GrupoDeFases> AntesDeCrear(int padreId, GrupoDeFasesDTO dto, GrupoDeFases entidad)
    {
        var torneo = await _torneoRepo.ObtenerPorId(padreId);
        if (torneo == null)
            throw new ExcepcionControlada("El torneo indicado no existe.");

        await ValidarPadreGrupo(padreId, dto.GrupoDeFasesPadreId);

        if (dto.Numero < 1)
            throw new ExcepcionControlada("El número del grupo debe ser mayor o igual a 1.");

        entidad.TorneoId = padreId;
        entidad.EsVisibleEnApp = true;
        return entidad;
    }

    protected override async Task AntesDeModificar(
        int padreId,
        int id,
        GrupoDeFasesDTO dto,
        GrupoDeFases entidadAnterior,
        GrupoDeFases entidadNueva)
    {
        await ValidarPadreGrupo(padreId, dto.GrupoDeFasesPadreId, id);

        if (dto.Numero < 1)
            throw new ExcepcionControlada("El número del grupo debe ser mayor o igual a 1.");

        entidadNueva.TorneoId = padreId;
    }

    public override async Task<int> Eliminar(int padreId, int id)
    {
        var grupos = await Repo.ListarTodosPorTorneoParaEditar(padreId);
        var entidad = grupos.FirstOrDefault(g => g.Id == id);
        if (entidad == null)
            return -1;

        var fases = await _faseRepo.ListarPorPadreParaEditar(padreId);

        var fasesParaMover = new List<Fase>();
        RecolectarFasesDelGrupo(fases, grupos, id, fasesParaMover);

        var numeroBase = entidad.Numero;
        for (var i = 0; i < fasesParaMover.Count; i++)
        {
            fasesParaMover[i].GrupoDeFasesId = entidad.GrupoDeFasesPadreId;
            fasesParaMover[i].Numero = numeroBase + i;
        }

        await BDVirtual.GuardarCambios();

        EliminarSubgruposRecursivo(grupos, id);

        Repo.Eliminar(entidad);
        await BDVirtual.GuardarCambios();
        return id;
    }

    private static void RecolectarFasesDelGrupo(
        IReadOnlyList<Fase> fases,
        IReadOnlyList<GrupoDeFases> grupos,
        int grupoId,
        List<Fase> destino)
    {
        destino.AddRange(
            fases.Where(f => f.GrupoDeFasesId == grupoId).OrderBy(f => f.Numero));

        foreach (var sub in grupos.Where(g => g.GrupoDeFasesPadreId == grupoId).OrderBy(g => g.Numero))
            RecolectarFasesDelGrupo(fases, grupos, sub.Id, destino);
    }

    private void EliminarSubgruposRecursivo(List<GrupoDeFases> grupos, int grupoId)
    {
        var subgrupos = grupos.Where(g => g.GrupoDeFasesPadreId == grupoId).ToList();
        foreach (var sub in subgrupos)
        {
            EliminarSubgruposRecursivo(grupos, sub.Id);
            Repo.Eliminar(sub);
            grupos.Remove(sub);
        }
    }

    private async Task ValidarPadreGrupo(int torneoId, int? padreGrupoId, int? grupoActualId = null)
    {
        if (padreGrupoId == null)
            return;

        var padre = await Repo.ObtenerPorIdYPadre(torneoId, padreGrupoId.Value);
        if (padre == null)
            throw new ExcepcionControlada("El grupo padre indicado no existe o no pertenece al torneo.");

        if (padre.GrupoDeFasesPadreId != null)
            throw new ExcepcionControlada("Solo se pueden crear subgrupos dentro de un grupo raíz.");

        if (grupoActualId != null && padreGrupoId == grupoActualId)
            throw new ExcepcionControlada("Un grupo no puede ser padre de sí mismo.");
    }

    public async Task CambiarVisibilidadEnApp(int torneoId, int grupoId, bool esVisibleEnApp)
    {
        var filas = await Repo.ActualizarEsVisibleEnApp(torneoId, grupoId, esVisibleEnApp);
        if (filas == 0)
            throw new ExcepcionControlada("No existe el grupo de fases a modificar o no pertenece al torneo indicado.");
    }
}
