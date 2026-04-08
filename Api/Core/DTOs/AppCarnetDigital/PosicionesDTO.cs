namespace Api.Core.DTOs.AppCarnetDigital;

public class PosicionesDTO
{
    public ICollection<CategoriasConPosicionesDTO> Posiciones { get; set; } = [];

    public bool VerGoles { get; set; }
}

public class CategoriasConPosicionesDTO
{
    public string Categoria { get; set; } = string.Empty;
    public ICollection<PosicionDelEquipoDTO> Renglones { get; set; } = [];
}

public class PosicionDelEquipoDTO
{
    public string Posicion { get; set; } = string.Empty;
    public string Escudo { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string PartidosJugados { get; set; } = string.Empty;
    public string PartidosGanados { get; set; } = string.Empty;
    public string PartidosEmpatados { get; set; } = string.Empty;
    public string PartidosPerdidos { get; set; } = string.Empty;
    public string GolesAFavor { get; set; } = string.Empty;
    public string GolesEnContra { get; set; } = string.Empty;
    public string GolesDiferencia { get; set; } = string.Empty;
    public string Puntos { get; set; } = string.Empty;
    public string PartidosNoPresento { get; set; } = string.Empty;
}

