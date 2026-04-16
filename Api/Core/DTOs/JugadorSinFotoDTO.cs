namespace Api.Core.DTOs;

public class JugadorSinFotoDTO
{
    public string DNI { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public List<DelegadoDeClubDTO> Delegados { get; set; } = new();
}

public class DelegadoDeClubDTO
{
    public string DNI { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? TelefonoCelular { get; set; }
    public string? Email { get; set; }
    public string NombreClub { get; set; } = string.Empty;
}
