using System.Text.Json;
using Api.Core.DTOs;
using Api.Core.Enums;

namespace Api.Core.Otros;

public static class PermisosClaimHelper
{
    public const string ClaimType = "permisos";

    public static string Serializar(IEnumerable<UsuarioAccesoModuloDTO> accesos) =>
        JsonSerializer.Serialize(accesos.Select(a => new PermisoClaimItem
        {
            Modulo = a.Modulo,
            Nivel = a.Nivel
        }));

    public static List<UsuarioAccesoModuloDTO> Deserializar(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            var items = JsonSerializer.Deserialize<List<PermisoClaimItem>>(json);
            return (items ?? [])
                .Select(i => new UsuarioAccesoModuloDTO { Modulo = i.Modulo, Nivel = i.Nivel })
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    private sealed class PermisoClaimItem
    {
        public ModuloSistema Modulo { get; set; }
        public NivelAcceso Nivel { get; set; }
    }
}
