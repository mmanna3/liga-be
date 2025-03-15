using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Core.Entidades;

public class HistorialDePagos
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int JugadorEquipoId { get; set; }
    
    [ForeignKey("JugadorEquipoId")]
    public JugadorEquipo? JugadorEquipo { get; set; }
    
    [Required]
    public DateTime Fecha { get; set; }
} 