using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;

namespace Api.TestsUtilidades;

public static class PermisosDePrueba
{
    public static List<UsuarioAccesoModuloDTO> AccesosTodosControlTotal() =>
        Enum.GetValues<ModuloSistema>()
            .Select(m => new UsuarioAccesoModuloDTO
            {
                Modulo = m,
                Nivel = NivelAcceso.ControlTotal
            })
            .ToList();

    public static List<UsuarioAccesoModuloDTO> AccesoSolo(ModuloSistema modulo, NivelAcceso nivel) =>
        [new UsuarioAccesoModuloDTO { Modulo = modulo, Nivel = nivel }];

    public static void SembrarAccesosControlTotal(AppDbContext context, int usuarioId)
    {
        var existentes = context.UsuarioAccesoModulo.Where(a => a.UsuarioId == usuarioId).ToList();
        context.UsuarioAccesoModulo.RemoveRange(existentes);

        foreach (var modulo in Enum.GetValues<ModuloSistema>())
        {
            context.UsuarioAccesoModulo.Add(new UsuarioAccesoModulo
            {
                Id = 0,
                UsuarioId = usuarioId,
                Modulo = modulo,
                Nivel = NivelAcceso.ControlTotal
            });
        }

        context.SaveChanges();
    }

    public static void SembrarAccesos(
        AppDbContext context,
        int usuarioId,
        IEnumerable<UsuarioAccesoModuloDTO> accesos)
    {
        var existentes = context.UsuarioAccesoModulo.Where(a => a.UsuarioId == usuarioId).ToList();
        context.UsuarioAccesoModulo.RemoveRange(existentes);

        foreach (var acceso in accesos)
        {
            context.UsuarioAccesoModulo.Add(new UsuarioAccesoModulo
            {
                Id = 0,
                UsuarioId = usuarioId,
                Modulo = acceso.Modulo,
                Nivel = acceso.Nivel
            });
        }

        context.SaveChanges();
    }
}
