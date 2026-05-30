using Api.Core.Entidades.EntidadesConValoresPredefinidos;
using Api.Core.Enums;

namespace Api.Core.Otros;

public static class CanchaTipoEtiqueta
{
    public static string Formatear(CanchaTipo? canchaTipo) =>
        Formatear(canchaTipo?.Tipo);

    public static string Formatear(string? tipo) => tipo switch
    {
        nameof(CanchaTipoEnum.PastoNatural) => "Pasto Natural",
        nameof(CanchaTipoEnum.PastoSintetico) => "Pasto Sintético",
        null or "" => nameof(CanchaTipoEnum.Consultar),
        _ => tipo
    };
}
