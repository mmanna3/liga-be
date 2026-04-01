using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios;
using Moq;

namespace Api.TestsUnitarios;

public class TorneoFaseCoreEliminarTests
{
    private static TorneoFaseCore CrearCore(Mock<ITorneoFaseRepo> repoMock)
    {
        var bdMock = new Mock<IBDVirtual>();
        var torneoRepoMock = new Mock<ITorneoRepo>();
        var mapperMock = new Mock<AutoMapper.IMapper>();
        return new TorneoFaseCore(bdMock.Object, repoMock.Object, torneoRepoMock.Object, mapperMock.Object);
    }

    private static FaseTodosContraTodos FaseConNumero(int id, int numero) =>
        new() { Id = id, Nombre = $"Fase {numero}", Numero = numero, TorneoId = 1, EstadoFaseId = 100 };

    // Casos: (faseIdAEliminar, numeroDeLaFaseEliminada)
    // El test verifica que DecrementarNumeroDeFasesPosteriores se llame
    // con el número correcto de la fase eliminada.
    public static TheoryData<int, int> CasosEliminacion => new()
    {
        // Eliminar la primera fase de cuatro (1,2,3,4 → eliminar #1)
        { 10, 1 },
        // Eliminar la segunda fase de cuatro (1,2,3,4 → eliminar #2)
        { 20, 2 },
        // Eliminar la tercera fase de cuatro (1,2,3,4 → eliminar #3)
        { 30, 3 },
        // Eliminar la última fase de cuatro (1,2,3,4 → eliminar #4)
        { 40, 4 },
        // Eliminar la única fase existente
        { 50, 1 },
    };

    [Theory]
    [MemberData(nameof(CasosEliminacion))]
    public async Task Eliminar_LlamaDecrementar_ConElNumeroCorrectoDelaFaseEliminada(
        int faseId, int numeroEsperado)
    {
        const int torneoId = 1;
        var repoMock = new Mock<ITorneoFaseRepo>();

        repoMock
            .Setup(r => r.ObtenerPorIdYPadre(torneoId, faseId))
            .ReturnsAsync(FaseConNumero(faseId, numeroEsperado));

        repoMock
            .Setup(r => r.DecrementarNumeroDeFasesPosteriores(torneoId, numeroEsperado))
            .Returns(Task.CompletedTask);

        var core = CrearCore(repoMock);

        await core.Eliminar(torneoId, faseId);

        repoMock.Verify(
            r => r.DecrementarNumeroDeFasesPosteriores(torneoId, numeroEsperado),
            Times.Once);
    }

    [Fact]
    public async Task Eliminar_FasesDelMedio_RenumeraCorrectamente()
    {
        // Fases: 1,2,3,4 — eliminar fase con numero=2
        // Debe llamar DecrementarNumeroDeFasesPosteriores(torneoId, 2)
        // para que las fases 3 y 4 pasen a ser 2 y 3.
        const int torneoId = 10;
        const int faseId = 20;
        const int numeroEliminado = 2;

        var repoMock = new Mock<ITorneoFaseRepo>();
        repoMock
            .Setup(r => r.ObtenerPorIdYPadre(torneoId, faseId))
            .ReturnsAsync(FaseConNumero(faseId, numeroEliminado));

        var core = CrearCore(repoMock);

        await core.Eliminar(torneoId, faseId);

        repoMock.Verify(
            r => r.DecrementarNumeroDeFasesPosteriores(torneoId, 2),
            Times.Once);
    }

    [Fact]
    public async Task Eliminar_FaseInexistente_NoLlamaDecrementar()
    {
        const int torneoId = 1;
        var repoMock = new Mock<ITorneoFaseRepo>();
        repoMock
            .Setup(r => r.ObtenerPorIdYPadre(torneoId, 999))
            .ReturnsAsync((TorneoFase?)null);

        var core = CrearCore(repoMock);

        await core.Eliminar(torneoId, 999);

        repoMock.Verify(
            r => r.DecrementarNumeroDeFasesPosteriores(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }
}
