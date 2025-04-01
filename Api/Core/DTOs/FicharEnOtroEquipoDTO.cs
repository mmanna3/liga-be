using System.ComponentModel.DataAnnotations;

namespace Api.Core.DTOs;

public class FicharEnOtroEquipoDTO : DTO
{
    [Required, MaxLength(9)] 
    public string DNI { get; set; } = string.Empty;
    
    [Required]
    public string CodigoAlfanumerico { get; set; }
}