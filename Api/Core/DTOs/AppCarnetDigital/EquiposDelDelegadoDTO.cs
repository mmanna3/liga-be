namespace Api.Core.DTOs.AppCarnetDigital;

public class EquiposDelDelegadoDTO
{
    public required string Club { get; set; }
    
    public ICollection<EquipoBaseDTO>? Equipos { get; set; }
}

public class EquipoBaseDTO
{
    public required int Id { get; set; }
    public required string Nombre { get; set; }
    public required string Torneo { get; set; }
}
