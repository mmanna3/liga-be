using Microsoft.EntityFrameworkCore;

namespace Api.Core.Entidades;

[Index(nameof(ArbitroId), nameof(JornadaId), IsUnique = true)]
public class ArbitroJornada : Entidad
{
    public int ArbitroId { get; set; }
    public virtual Arbitro Arbitro { get; set; } = null!;
    public int JornadaId { get; set; }
    public virtual Jornada Jornada { get; set; } = null!;
    public required int Orden { get; set; }
    public bool WhatsappEnviado { get; set; }
    public string? WhatsappHorarioInicio { get; set; }
    public string? WhatsappObservaciones { get; set; }
    public string? WhatsappCategoriasJson { get; set; }
    public DateTime? WhatsappEnviadoEn { get; set; }
}
