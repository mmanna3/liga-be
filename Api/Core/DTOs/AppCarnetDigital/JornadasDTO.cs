namespace Api.Core.DTOs.AppCarnetDigital;

public class JornadasDTO
{
    /// <summary>
    /// Una entrada por cada fecha de la zona (título "Fecha N"); cada una agrupa los partidos de esa fecha.
    /// </summary>
    public ICollection<FechasParaJornadasDTO> Fechas { get; set; } = [];
}

public class FechasParaJornadasDTO
{
    public string Titulo { get; set; } = string.Empty;
    public string Dia { get; set; } = string.Empty;
    public ICollection<JornadasPorFechaDTO> Jornadas { get; set; } = [];
}

public class JornadasPorFechaDTO
{
    public JornadaPorEquipoDTO Local { get; set; } = null!;
    public JornadaPorEquipoDTO Visitante { get; set; } = null!;
}

public class JornadaPorEquipoDTO
{
    public string Escudo { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public ICollection<ResultadoCategoriaDTO> Categorias { get; set; } = [];
}

public class ResultadoCategoriaDTO
{
    public string Categoria { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
}
