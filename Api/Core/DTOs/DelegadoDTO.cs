namespace Api.Core.DTOs;

public class DelegadoDTO : DTO
{
    public required string Usuario { get; set; }
    public required string Nombre { get; set; }
    public required string Apellido { get; set; }
    public required string Password { get; set; }
    public virtual required int ClubId { get; set; }
}