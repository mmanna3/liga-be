using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Otros;

namespace Api.TestsUnitarios;

public class EstructuraFasesTreeBuilderTests
{
    [Fact]
    public void ConstruirElementos_SinGrupos_DevuelveFasesPlanas()
    {
        var fases = new List<Fase>
        {
            new FaseTodosContraTodos { Id = 1, Nombre = "A", Numero = 1, TorneoId = 1, EstadoFaseId = 100, EsVisibleEnApp = true },
            new FaseTodosContraTodos { Id = 2, Nombre = "B", Numero = 2, TorneoId = 1, EstadoFaseId = 100, EsVisibleEnApp = true }
        };

        var elementos = EstructuraFasesTreeBuilder.ConstruirElementosDesdeEntidades(fases, []);

        Assert.Equal(2, elementos.Count);
        Assert.All(elementos, e => Assert.Equal(EstructuraFasesTreeBuilder.TipoFase, e.Tipo));
        Assert.Equal(1, elementos[0].FaseId);
        Assert.Equal(2, elementos[1].FaseId);
    }

    [Fact]
    public void ConstruirElementosInformacionInicial_ConGrupoAnidado()
    {
        var grupoA = new GrupoDeFases { Id = 10, Nombre = "Grupo A", Numero = 1, TorneoId = 1, EsVisibleEnApp = true };
        var grupoB = new GrupoDeFases { Id = 11, Nombre = "Grupo B", Numero = 1, TorneoId = 1, GrupoDeFasesPadreId = 10, EsVisibleEnApp = true };
        var faseD = new FaseTodosContraTodos { Id = 4, Nombre = "D", Numero = 1, TorneoId = 1, GrupoDeFasesId = 11, EstadoFaseId = 100, EsVisibleEnApp = true };

        var zonas = new Dictionary<int, List<InformacionInicialZonaDTO>>
        {
            [4] = [new InformacionInicialZonaDTO { Id = 100, Nombre = "Zona D", Orden = 1 }]
        };

        var elementos = EstructuraFasesTreeBuilder.ConstruirElementosInformacionInicial(
            [faseD], [grupoA, grupoB], zonas);

        var grupoEl = Assert.Single(elementos);
        Assert.Equal(EstructuraFasesTreeBuilder.TipoGrupo, grupoEl.Tipo);
        Assert.Equal("Grupo A", grupoEl.NombreGrupo);

        var subGrupo = Assert.Single(grupoEl.Elementos!);
        Assert.Equal(EstructuraFasesTreeBuilder.TipoGrupo, subGrupo.Tipo);

        var faseEl = Assert.Single(subGrupo.Elementos!);
        Assert.Equal("D", faseEl.Nombre);
        Assert.Single(faseEl.Zonas!);
    }

    [Fact]
    public void AplanarFasesInformacionInicial_AchataGruposManteniendoOrden()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            new()
            {
                Tipo = EstructuraFasesTreeBuilder.TipoFase,
                Id = 1,
                Nombre = "Fase A",
                TipoDeFase = nameof(TipoDeFaseEnum.TodosContraTodos)
            },
            new()
            {
                Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                GrupoId = 10,
                NombreGrupo = "Grupo A",
                Elementos =
                [
                    new InformacionInicialElementoTorneoDTO
                    {
                        Tipo = EstructuraFasesTreeBuilder.TipoFase,
                        Id = 2,
                        Nombre = "Fase B",
                        TipoDeFase = nameof(TipoDeFaseEnum.TodosContraTodos)
                    },
                    new InformacionInicialElementoTorneoDTO
                    {
                        Tipo = EstructuraFasesTreeBuilder.TipoFase,
                        Id = 3,
                        Nombre = "Fase C",
                        TipoDeFase = nameof(TipoDeFaseEnum.TodosContraTodos)
                    }
                ]
            },
            new()
            {
                Tipo = EstructuraFasesTreeBuilder.TipoFase,
                Id = 4,
                Nombre = "Fase F",
                TipoDeFase = nameof(TipoDeFaseEnum.TodosContraTodos)
            }
        };

        var fases = EstructuraFasesTreeBuilder.AplanarFasesInformacionInicial(elementos);

        Assert.Equal(["Fase A", "Fase B", "Fase C", "Fase F"], fases.Select(f => f.Nombre).ToList());
        Assert.All(fases, f => Assert.Equal(nameof(TipoDeFaseEnum.TodosContraTodos), f.TipoDeFase));
    }

    [Fact]
    public void ValidarProfundidad_RechazaTercerNivel()
    {
        var items = new List<EstructuraFasesItemDTO>
        {
            new()
            {
                Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                GrupoId = 1,
                Items =
                [
                    new EstructuraFasesItemDTO
                    {
                        Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                        GrupoId = 2,
                        Items =
                        [
                            new EstructuraFasesItemDTO
                            {
                                Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
                                GrupoId = 3,
                                Items = []
                            }
                        ]
                    }
                ]
            }
        };

        Assert.Throws<ExcepcionControlada>(() =>
            EstructuraFasesTreeBuilder.ValidarProfundidadEstructura(items));
    }
}
