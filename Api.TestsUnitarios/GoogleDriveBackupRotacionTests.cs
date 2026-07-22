using Api.Core.Servicios;
using Moq;

namespace Api.TestsUnitarios;

public class GoogleDriveBackupRotacionTests
{
    private static List<(string Id, string Name)> Pares(params string[] fechas) =>
        fechas.SelectMany((f, i) => new[]
        {
            ($"bd-{i}", $"backup-bd-{f}-02-00.zip"),
            ($"img-{i}", $"backup-imagenes-{f}-02-01.zip")
        }).ToList();

    [Fact]
    public void Viernes24_ConservaHoyAyerLunesYDia1_Borra22()
    {
        // Vie 24/7/2026 → keepers: 24, 23, 20 (lun), 01
        var archivos = Pares("2026-07-24", "2026-07-23", "2026-07-22", "2026-07-20", "2026-07-01");
        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

        Assert.Equal(5, r.ParesDetectados);
        Assert.Equal(2, r.IdsABorrar.Count);
        Assert.Contains("backup-bd-2026-07-22-02-00.zip", r.ArchivosBorrados);
        Assert.Contains("backup-imagenes-2026-07-22-02-01.zip", r.ArchivosBorrados);
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-24"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-23"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-20"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-01"));
    }

    [Fact]
    public void Martes21_LunesEnVentanaDiaria_ConservaLunesPrevio()
    {
        // Mar 21/7 → diarios 21 y 20 (lun); semanal = 13; mensual = 01
        var archivos = Pares("2026-07-21", "2026-07-20", "2026-07-13", "2026-07-01", "2026-07-19");
        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 21));

        Assert.Equal(5, r.ParesDetectados);
        Assert.Equal(2, r.IdsABorrar.Count);
        Assert.Contains("backup-bd-2026-07-19-02-00.zip", r.ArchivosBorrados);
        Assert.Contains("backup-imagenes-2026-07-19-02-01.zip", r.ArchivosBorrados);
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-13"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-20"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-21"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-01"));
    }

    [Fact]
    public void Dia1EnVentanaDiaria_ConservaDia1DelMesAnterior()
    {
        // Jue 2/7 → diarios 02 y 01; mensual = 01/06; semanal = lunes antes de 01
        var archivos = Pares("2026-07-02", "2026-07-01", "2026-06-29", "2026-06-01", "2026-06-15");
        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 2));

        Assert.Equal(5, r.ParesDetectados);
        Assert.Equal(2, r.IdsABorrar.Count);
        Assert.Contains("backup-bd-2026-06-15-02-00.zip", r.ArchivosBorrados);
        Assert.Contains("backup-imagenes-2026-06-15-02-01.zip", r.ArchivosBorrados);
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-02"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-07-01"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-06-29"));
        Assert.DoesNotContain(r.ArchivosBorrados, n => n.Contains("2026-06-01"));
    }

    [Fact]
    public void SoloFechasKeeper_NoBorraNada()
    {
        var archivos = Pares("2026-07-24", "2026-07-23", "2026-07-20", "2026-07-01");
        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

        Assert.Equal(4, r.ParesDetectados);
        Assert.Empty(r.IdsABorrar);
        Assert.Empty(r.ArchivosBorrados);
    }

    [Fact]
    public void ArchivoConNombreNoReconocido_NoSeBorra()
    {
        var archivos = Pares("2026-07-24", "2026-07-23", "2026-07-22");
        archivos.Add(("otro", "notas.txt"));

        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

        Assert.DoesNotContain("notas.txt", r.ArchivosBorrados);
        Assert.Contains("notas.txt", r.BackupsEnDrive);
        Assert.Contains("backup-bd-2026-07-22-02-00.zip", r.ArchivosBorrados);
    }

    [Fact]
    public void MismoDia_MinutosDistintosEntreBdEImagenes_CuentaUnSoloPar()
    {
        var archivos = new List<(string, string)>
        {
            ("a", "backup-bd-2026-07-24-02-16.zip"),
            ("b", "backup-imagenes-2026-07-24-02-17.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

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
    public void CalcularFechasAConservar_Viernes24()
    {
        var keepers = GoogleDriveBackupRotacion.CalcularFechasAConservar(new DateOnly(2026, 7, 24));

        Assert.Equal(
            new HashSet<string> { "2026-07-24", "2026-07-23", "2026-07-20", "2026-07-01" },
            keepers);
    }

    [Fact]
    public void CalcularFechasAConservar_Martes21_LunesPrevio()
    {
        var keepers = GoogleDriveBackupRotacion.CalcularFechasAConservar(new DateOnly(2026, 7, 21));

        Assert.Equal(
            new HashSet<string> { "2026-07-21", "2026-07-20", "2026-07-13", "2026-07-01" },
            keepers);
    }

    [Fact]
    public void BackupsEnDrive_ListaTodosLosNombresOrdenados()
    {
        var archivos = new List<(string, string)>
        {
            ("z", "zebra.zip"),
            ("a", "backup-bd-2026-07-24-02-16.zip")
        };

        var r = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

        Assert.Equal(new[] { "backup-bd-2026-07-24-02-16.zip", "zebra.zip" }, r.BackupsEnDrive);
    }

    /// <summary>
    /// Simula el bucle de <see cref="Api.Core.Servicios.GoogleDriveCore.RotarBackupsEnDrive"/>:
    /// una llamada a Delete por cada id en <see cref="RotacionBackupsDriveResult.IdsABorrar"/>.
    /// </summary>
    [Fact]
    public void SimulacionDelete_IgualCantidadQueIdsABorrar_YVerificacionMoq()
    {
        var archivos = Pares("2026-07-24", "2026-07-23", "2026-07-22", "2026-07-20", "2026-07-01");
        var plan = GoogleDriveBackupRotacion.Calcular(archivos, new DateOnly(2026, 7, 24));

        var deleter = new Mock<IDeleteArchivoDriveParaTest>();
        foreach (var id in plan.IdsABorrar)
            deleter.Object.Eliminar(id);

        deleter.Verify(d => d.Eliminar(It.IsAny<string>()), Times.Exactly(plan.IdsABorrar.Count));
        deleter.Verify(d => d.Eliminar("bd-2"), Times.Once());
        deleter.Verify(d => d.Eliminar("img-2"), Times.Once());
    }
}

/// <summary>Contrato mínimo para simular <c>Files.Delete(id)</c> en tests con Moq.</summary>
public interface IDeleteArchivoDriveParaTest
{
    void Eliminar(string id);
}
