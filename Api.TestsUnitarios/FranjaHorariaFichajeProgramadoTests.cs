using Api.Core.Servicios;

namespace Api.TestsUnitarios;

public class FranjaHorariaFichajeProgramadoTests
{
    [Fact]
    public void Lunes_antes_de_las_8_no_esta_activa()
    {
        var dt = new DateTime(2026, 4, 13, 7, 59, 59, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Monday, dt.DayOfWeek);

        Assert.False(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Lunes_a_las_8_exacto_esta_activa()
    {
        var dt = new DateTime(2026, 4, 13, 8, 0, 0, DateTimeKind.Unspecified);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Lunes_medio_dia_esta_activa()
    {
        var dt = new DateTime(2026, 4, 13, 14, 30, 0, DateTimeKind.Unspecified);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Martes_cualquier_hora_esta_activa()
    {
        var dt = new DateTime(2026, 4, 14, 0, 0, 1, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Tuesday, dt.DayOfWeek);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Miercoles_esta_activa()
    {
        var dt = new DateTime(2026, 4, 15, 23, 59, 0, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Wednesday, dt.DayOfWeek);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Jueves_antes_de_las_20_esta_activa()
    {
        var dt = new DateTime(2026, 4, 16, 19, 59, 59, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Thursday, dt.DayOfWeek);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Jueves_a_las_20_exacto_esta_activa()
    {
        var dt = new DateTime(2026, 4, 16, 20, 0, 0, DateTimeKind.Unspecified);

        Assert.True(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Jueves_justo_despues_de_las_20_no_esta_activa()
    {
        var dt = new DateTime(2026, 4, 16, 20, 0, 0, DateTimeKind.Unspecified).AddMilliseconds(1);

        Assert.False(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Viernes_no_esta_activa()
    {
        var dt = new DateTime(2026, 4, 17, 12, 0, 0, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Friday, dt.DayOfWeek);

        Assert.False(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Sabado_no_esta_activa()
    {
        var dt = new DateTime(2026, 4, 11, 10, 0, 0, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Saturday, dt.DayOfWeek);

        Assert.False(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }

    [Fact]
    public void Domingo_no_esta_activa()
    {
        var dt = new DateTime(2026, 4, 12, 15, 0, 0, DateTimeKind.Unspecified);
        Assert.Equal(DayOfWeek.Sunday, dt.DayOfWeek);

        Assert.False(FranjaHorariaFichajeProgramado.EstaActiva(dt));
    }
}
