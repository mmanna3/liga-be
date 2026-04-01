using Api.Core.Servicios;
using Moq;

namespace Api.TestsUnitarios;

public class GoogleDriveBackupRotacionTests
{
    private static readonly string[] NombresEjemploDrive =
    [
        "backup-imagenes-2026-03-31-02-17.zip",
        "backup-imagenes-2026-03-30-02-31.zip",
        "backup-imagenes-2026-03-28-01-59.zip",
        "backup-imagenes-2026-03-29-02-18.zip",
        "backup-bd-2026-03-31-02-16.zip",
        "backup-bd-2026-03-30-02-30.zip",
        "backup-bd-2026-03-29-02-18.zip",
        "backup-bd-2026-03-28-02-18.zip"
    ];

    [Fact]
    public void CuatroParesDeFechasDistintas_BorraSoloElDiaMasAntiguo_DosArchivos()
    {
        var archivos = NombresEjemploDrive
            .Select((n, i) => ($"id-{i}", n))
            .ToList();

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxPares: 3);

        Assert.Equal(4, r.ParesDetectados);
        Assert.Equal(2, r.IdsABorrar.Count);
        Assert.Equal(2, r.ArchivosBorrados.Count);

        Assert.Contains("backup-bd-2026-03-28-02-18.zip", r.ArchivosBorrados);
        Assert.Contains("backup-imagenes-2026-03-28-01-59.zip", r.ArchivosBorrados);
        // Orden alfabético por nombre: backup-bd-... antes que backup-imagenes-...
        Assert.Equal("id-7", r.IdsABorrar[0]);
        Assert.Equal("id-2", r.IdsABorrar[1]);
    }

    [Fact]
    public void TresDiasDistintos_NoBorraNada()
    {
        var nombres = new[]
        {
            "backup-bd-2026-03-29-02-18.zip",
            "backup-bd-2026-03-30-02-30.zip",
            "backup-bd-2026-03-31-02-16.zip",
            "backup-imagenes-2026-03-29-02-18.zip",
            "backup-imagenes-2026-03-30-02-31.zip",
            "backup-imagenes-2026-03-31-02-17.zip"
        };
        var archivos = nombres.Select((n, i) => ($"id-{i}", n)).ToList();

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxPares: 3);

        Assert.Equal(3, r.ParesDetectados);
        Assert.Empty(r.IdsABorrar);
        Assert.Empty(r.ArchivosBorrados);
    }

    [Fact]
    public void MismoDia_MinutosDistintosEntreBdEImagenes_CuentaUnSoloPar()
    {
        var archivos = new List<(string, string)>
        {
            ("a", "backup-bd-2026-03-31-02-16.zip"),
            ("b", "backup-imagenes-2026-03-31-02-17.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos, maxPares: 3);

        Assert.Equal(1, r.ParesDetectados);
        Assert.Empty(r.IdsABorrar);
    }

    [Fact]
    public void ExtraerFechaDesdeNombreBackup_DevuelveSoloYyyyMmDd()
    {
        Assert.Equal("2026-03-28", GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("backup-bd-2026-03-28-02-18.zip"));
        Assert.Equal("2026-03-28", GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("backup-imagenes-2026-03-28-01-59.zip"));
        Assert.Null(GoogleDriveBackupRotacion.ExtraerFechaDesdeNombreBackup("otro.zip"));
    }

    [Fact]
    public void BackupsEnDrive_ListaTodosLosNombresOrdenados()
    {
        var archivos = new List<(string, string)>
        {
            ("z", "zebra.zip"),
            ("a", "backup-bd-2026-03-31-02-16.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos);

        Assert.Equal(new[] { "backup-bd-2026-03-31-02-16.zip", "zebra.zip" }, r.BackupsEnDrive);
    }

    /// <summary>
    /// Simula el bucle de <see cref="Api.Core.Servicios.GoogleDriveCore.RotarBackupsEnDrive"/>:
    /// una llamada a Delete por cada id en <see cref="RotacionBackupsDriveResult.IdsABorrar"/>.
    /// </summary>
    [Fact]
    public void SimulacionDelete_IgualCantidadQueIdsABorrar_YVerificacionMoq()
    {
        var archivos = NombresEjemploDrive
            .Select((n, i) => ($"id-{i}", n))
            .ToList();
        var plan = GoogleDriveBackupRotacion.Calcular(archivos, maxPares: 3);

        var deleter = new Mock<IDeleteArchivoDriveParaTest>();
        foreach (var id in plan.IdsABorrar)
            deleter.Object.Eliminar(id);

        deleter.Verify(d => d.Eliminar(It.IsAny<string>()), Times.Exactly(plan.IdsABorrar.Count));
        deleter.Verify(d => d.Eliminar("id-7"), Times.Once());
        deleter.Verify(d => d.Eliminar("id-2"), Times.Once());
    }
}

/// <summary>Contrato mínimo para simular <c>Files.Delete(id)</c> en tests con Moq.</summary>
public interface IDeleteArchivoDriveParaTest
{
    void Eliminar(string id);
}
