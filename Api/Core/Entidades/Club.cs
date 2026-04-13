using System.ComponentModel.DataAnnotations;
using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;

namespace Api.Core.Entidades;

public class Club : Entidad
{
    [MaxLength(100)]
    public required string Nombre { get; set; }

    [MaxLength(100)]
    public string? Localidad { get; set; }

    [MaxLength(150)]
    public string? Direccion { get; set; }

    public bool? EsTechado { get; set; }

    public int CanchaTipoId { get; set; } = (int)CanchaTipoEnum.Consultar;

    public virtual CanchaTipo CanchaTipo { get; set; } = null!;

    public virtual ICollection<Equipo> Equipos { get; set; } = null!;
    public virtual ICollection<DelegadoClub> DelegadoClubs { get; set; } = null!;
}