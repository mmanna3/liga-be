using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios;

public class PublicoCore : IPublicoCore
{
    private readonly IJugadorRepo _jugadorRepo;
    private readonly IJugadorCore _jugadorCore;
    private readonly IDelegadoRepo _delegadoRepo;
    private readonly IImagenJugadorRepo _imagenJugadorRepo;
    private readonly IBDVirtual _bdVirtual;
    private readonly IDniExpulsadoDeLaLigaRepo _dniExpulsadoDeLaLigaRepo;

    public PublicoCore(IJugadorRepo jugadorRepo, IJugadorCore jugadorCore, IDelegadoRepo delegadoRepo, IImagenJugadorRepo imagenJugadorRepo, IBDVirtual bdVirtual, IDniExpulsadoDeLaLigaRepo dniExpulsadoDeLaLigaRepo)
    {
        _jugadorRepo = jugadorRepo;
        _jugadorCore = jugadorCore;
        _delegadoRepo = delegadoRepo;
        _imagenJugadorRepo = imagenJugadorRepo;
        _bdVirtual = bdVirtual;
        _dniExpulsadoDeLaLigaRepo = dniExpulsadoDeLaLigaRepo;
    }

    public async Task<bool> ElDniEstaFichado(string dni)
    {
        var jugador = await _jugadorRepo.ObtenerPorDNI(dni);
        var delegado = await _delegadoRepo.ObtenerPorDNI(dni);

        return PersonaExisteHelper.JugadorExiste(jugador) || PersonaExisteHelper.DelegadoExiste(delegado);
    }

    public async Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto)
    {
        var dniLimpio = new string(dto.DNI.Where(char.IsDigit).ToArray());
        await ValidacionDniExpulsado.LanzarSiEstaExpulsado(_dniExpulsadoDeLaLigaRepo, dniLimpio);

        var dniParaBuscar = string.IsNullOrEmpty(dniLimpio) ? dto.DNI : dniLimpio;
        var jugador = await _jugadorRepo.ObtenerPorDNI(dniParaBuscar);
        if (PersonaExisteHelper.JugadorEstaPendiente(jugador))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como jugador. La administración debe aprobarlo antes de poder fichar en otro equipo.");

        var delegado = await _delegadoRepo.ObtenerPorDNI(dniParaBuscar);
        if (PersonaExisteHelper.DelegadoEstaPendiente(delegado))
            throw new ExcepcionControlada("El DNI está pendiente de aprobación como delegado. La administración debe aprobarlo antes de poder fichar en otro equipo.");

        if (PersonaExisteHelper.JugadorExiste(jugador))
        {
            var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(dto.CodigoAlfanumerico);
            await _jugadorCore.FicharJugadorEnElEquipo(equipoId, jugador!);
            await _bdVirtual.GuardarCambios();
            return jugador!.Id;
        }

        if (PersonaExisteHelper.DelegadoExiste(delegado))
        {
            return await FicharJugadorDesdeDelegado(delegado!, dto.CodigoAlfanumerico);
        }

        throw new ExcepcionControlada("No existe ni un jugador ni un delegado con el DNI indicado.");
    }

    public async Task<string> ListarJugadoresSinFoto()
    {
        var todos = await _jugadorRepo.ListarJugadorEquiposNoPendientesConRelaciones();
        var sinFoto = todos.Where(je => !_imagenJugadorRepo.TieneFoto(je.Jugador.DNI)).ToList();

        var anoActual = DateTime.Now.Year;
        var deEsteAno = sinFoto.Where(je => je.FechaFichaje.Year == anoActual).ToList();

        var sb = new System.Text.StringBuilder();

        var totalJugadores = sinFoto.Select(je => je.Jugador.DNI).Distinct().Count();
        var totalEquipos = sinFoto.Select(je => je.EquipoId).Distinct().Count();
        var jugadoresAno = deEsteAno.Select(je => je.Jugador.DNI).Distinct().Count();
        var equiposAno = deEsteAno.Select(je => je.EquipoId).Distinct().Count();

        sb.AppendLine($"De todos los años, hay {totalJugadores} jugadores sin foto en {totalEquipos} equipos");
        sb.AppendLine($"Fichados en {anoActual}, hay {jugadoresAno} jugadores sin foto en {equiposAno} equipos");

        AppendSeccion(sb, $"Fichados en {anoActual}", deEsteAno);
        AppendSeccion(sb, $"Fichados antes de {anoActual}", sinFoto.Where(je => je.FechaFichaje.Year < anoActual).ToList());

        return sb.ToString();
    }

    private static void AppendSeccion(System.Text.StringBuilder sb, string titulo, List<JugadorEquipo> items)
    {
        sb.AppendLine();
        sb.AppendLine(titulo);

        var porEquipo = items
            .GroupBy(je => je.EquipoId)
            .OrderBy(g => g.First().Equipo.Nombre);

        foreach (var grupo in porEquipo)
        {
            var equipo = grupo.First().Equipo;
            var delegados = equipo.Club.DelegadoClubs
                .Select(dc => $"{dc.Delegado.Nombre} {dc.Delegado.Apellido} {dc.Delegado.TelefonoCelular}".Trim())
                .ToList();
            var delegadosStr = delegados.Count > 0 ? string.Join(" | ", delegados) : "(sin delegados)";

            sb.AppendLine($"{equipo.Nombre} - {delegadosStr}");

            foreach (var je in grupo.OrderBy(je => je.FechaFichaje))
                sb.AppendLine($"  {je.Jugador.DNI} {je.Jugador.Nombre} {je.Jugador.Apellido} - {je.FechaFichaje:dd/MM/yyyy}");
        }
    }

    private async Task<int> FicharJugadorDesdeDelegado(Delegado delegado, string codigoAlfanumerico)
    {
        _imagenJugadorRepo.CopiarFotosDeDelegadoATemporales(delegado.DNI);

        var jugador = new Jugador
        {
            Id = 0,
            DNI = delegado.DNI,
            Nombre = delegado.Nombre,
            Apellido = delegado.Apellido,
            FechaNacimiento = delegado.FechaNacimiento
        };

        var equipoId = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        await _jugadorCore.FicharJugadorEnElEquipo(equipoId, jugador);

        _jugadorRepo.Crear(jugador);
        await _bdVirtual.GuardarCambios();

        return jugador.Id;
    }
}