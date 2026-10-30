namespace Api.Api.Authorization;

/// <summary>
/// Habilita el endpoint para el rol Delegado. Sin este atributo, un delegado recibe 403
/// aunque los roles del [Authorize] lo incluyan (ver RestriccionDelegadoFilter).
/// Solo va en lo que usa la app de delegados.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermitirDelegadoAttribute : Attribute
{
}
