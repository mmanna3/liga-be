using Api.Core.Enums;

namespace Api.Api.Authorization;

/// <summary>
/// Asocia el controller a un módulo del sistema para validar permisos por acción HTTP.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ModuloSistemaAttribute : Attribute
{
    public ModuloSistema Modulo { get; }

    public ModuloSistemaAttribute(ModuloSistema modulo)
    {
        Modulo = modulo;
    }
}
