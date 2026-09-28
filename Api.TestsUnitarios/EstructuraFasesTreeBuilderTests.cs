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

    private static InformacionInicialElementoTorneoDTO ElementoFase(int id, string nombre) => new()
    {
        Tipo = EstructuraFasesTreeBuilder.TipoFase,
        Id = id,
        Nombre = nombre,
        TipoDeFase = nameof(TipoDeFaseEnum.TodosContraTodos)
    };

    private static InformacionInicialElementoTorneoDTO ElementoGrupo(int id, string nombre,
        params InformacionInicialElementoTorneoDTO[] hijos) => new()
    {
        Tipo = EstructuraFasesTreeBuilder.TipoGrupo,
        GrupoId = id,
        NombreGrupo = nombre,
        Elementos = hijos.ToList()
    };

    private static InformacionInicialElementoTorneoDTO ElementoAnual() => new()
    {
        Tipo = EstructuraFasesTreeBuilder.TipoFase,
        Id = 0,
        Nombre = "Anual",
        TipoDeFase = "Anual"
    };

    private static List<string> Nombres(IEnumerable<InformacionInicialElementoTorneoDTO> elementos) =>
        elementos.Select(e => e.Tipo == EstructuraFasesTreeBuilder.TipoGrupo ? e.NombreGrupo! : e.Nombre!).ToList();

    [Fact]
    public void InsertarFaseAnual_AperturaYClausuraEnMismoGrupo_QuedaComoHermanaDentroDelGrupo()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoGrupo(10, "Torneo anual", ElementoFase(1, "Apertura"), ElementoFase(2, "Clausura")),
            ElementoFase(3, "Copa")
        };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?> { [10] = null }, (1, 10), (2, 10), ElementoAnual());

        Assert.Equal(["Torneo anual", "Copa"], Nombres(elementos));
        Assert.Equal(["Apertura", "Clausura", "Anual"], Nombres(elementos[0].Elementos!));
    }

    [Fact]
    public void InsertarFaseAnual_ConFasesPosteriores_QuedaJustoDespuesDeLaUltima()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoGrupo(10, "Torneo anual", ElementoFase(1, "Apertura"), ElementoFase(2, "Clausura"), ElementoFase(3, "Playoffs"))
        };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?> { [10] = null }, (1, 10), (2, 10), ElementoAnual());

        Assert.Equal(["Apertura", "Clausura", "Anual", "Playoffs"], Nombres(elementos[0].Elementos!));
    }

    [Fact]
    public void InsertarFaseAnual_ClausuraAntesQueApertura_QuedaDespuesDeApertura()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoFase(2, "Clausura"), ElementoFase(1, "Apertura"), ElementoFase(3, "Copa")
        };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?>(), (1, null), (2, null), ElementoAnual());

        Assert.Equal(["Clausura", "Apertura", "Anual", "Copa"], Nombres(elementos));
    }

    [Fact]
    public void InsertarFaseAnual_EnSubgruposDistintos_QuedaEnElGrupoPadreDespuesDelUltimoSubgrupo()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoGrupo(10, "Torneo anual",
                ElementoGrupo(11, "Primera mitad", ElementoFase(1, "Apertura")),
                ElementoGrupo(12, "Segunda mitad", ElementoFase(2, "Clausura")),
                ElementoFase(3, "Final"))
        };
        var padres = new Dictionary<int, int?> { [10] = null, [11] = 10, [12] = 10 };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(elementos, padres, (1, 11), (2, 12), ElementoAnual());

        Assert.Equal(["Primera mitad", "Segunda mitad", "Anual", "Final"], Nombres(elementos[0].Elementos!));
    }

    [Fact]
    public void InsertarFaseAnual_SinGrupoComun_QuedaEnRaizDespuesDelUltimoContenedor()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoGrupo(10, "Grupo A", ElementoFase(1, "Apertura")),
            ElementoFase(2, "Clausura"),
            ElementoFase(3, "Copa")
        };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?> { [10] = null }, (1, 10), (2, null), ElementoAnual());

        Assert.Equal(["Grupo A", "Clausura", "Anual", "Copa"], Nombres(elementos));
        Assert.Equal(["Apertura"], Nombres(elementos[0].Elementos!));
    }

    [Fact]
    public void InsertarFaseAnual_GrupoComunNoVisible_QuedaAlFinalDelContenedorVisible()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO> { ElementoFase(3, "Copa") };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?> { [10] = null }, (1, 10), (2, 10), ElementoAnual());

        Assert.Equal(["Copa", "Anual"], Nombres(elementos));
    }

    [Fact]
    public void InsertarFaseAnual_ClausuraNoVisible_QuedaDespuesDeApertura()
    {
        var elementos = new List<InformacionInicialElementoTorneoDTO>
        {
            ElementoGrupo(10, "Torneo anual", ElementoFase(1, "Apertura"), ElementoFase(3, "Playoffs"))
        };

        EstructuraFasesTreeBuilder.InsertarFaseAnual(
            elementos, new Dictionary<int, int?> { [10] = null }, (1, 10), (2, 10), ElementoAnual());

        Assert.Equal(["Apertura", "Anual", "Playoffs"], Nombres(elementos[0].Elementos!));
    }
}
