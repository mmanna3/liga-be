using Api.Core.Repositorios;

namespace Api.Core.Otros;

public static class ValidacionDniExpulsado
{
    public const string MensajeNoHabilitadoParaFichaje = "Este DNI no está habilitado para fichaje";

    /// <summary>
    /// Si el DNI (solo dígitos) figura como expulsado de la liga, lanza <see cref="ExcepcionControlada"/>.
    /// </summary>
    public static async Task LanzarSiEstaExpulsado(IDniExpulsadoDeLaLigaRepo repo, string dniSoloDigitos,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(dniSoloDigitos) || !int.TryParse(dniSoloDigitos, out var dni))
            return;

        if (await repo.ExistePorDniAsync(dni, cancellationToken))
            throw new ExcepcionControlada(MensajeNoHabilitadoParaFichaje);
    }
}
