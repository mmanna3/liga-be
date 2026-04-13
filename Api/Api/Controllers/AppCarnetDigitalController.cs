using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Api.Api.Authorization;

namespace Api.Api.Controllers
{
    [Route("api/carnet-digital")]
    [ApiController]
    [Authorize]
    public class AppCarnetDigitalController : ControllerBase
    {
        private readonly IAppCarnetDigitalCore _core;

        public AppCarnetDigitalController(IAppCarnetDigitalCore core)
        {
            _core = core;
        }

        [HttpGet("equipos-del-delegado")]
        public async Task<ActionResult<EquiposDelDelegadoDTO>> Equipos()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var usuario = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (usuario != null)
                {
                    var equipos = await _core.ObtenerEquiposPorUsuarioDeDelegado(usuario);
                    return Ok(equipos);
                }
            }

            return Unauthorized("No se pudo obtener el usuario del token.");
        }

        [HttpGet("carnets")]
        public async Task<ActionResult<ICollection<CarnetDigitalDTO>>> Carnets(int equipoId)
        {
            var equipos = await _core.Carnets(equipoId);

            return Ok(equipos);
        }

        [HttpGet("carnets-por-codigo-alfanumerico")]
        public async Task<ActionResult<ICollection<CarnetDigitalDTO>>> CarnetsPorCodigoAlfanumerico(string codigoAlfanumerico)
        {
            var equipos = await _core.CarnetsPorCodigoAlfanumerico(codigoAlfanumerico);

            return Ok(equipos);
        }

        /// <summary>Planillas de juego por categoría del torneo (jugadores activos, suspendidos o inhabilitados).</summary>
        [HttpGet("planillas-de-juego")]
        public async Task<ActionResult<PlanillaDeJuegoDTO>> PlanillasDeJuego([FromQuery] string codigoAlfanumerico,
            CancellationToken cancellationToken)
        {
            var datos = await _core.PlanillasDeJuegoAsync(codigoAlfanumerico, cancellationToken);
            return Ok(datos);
        }

        [HttpGet("jugadores-pendientes")]
        public async Task<ActionResult<ICollection<CarnetDigitalPendienteDTO>>> JugadoresPendientes(int equipoId)
        {
            var equipos = await _core.JugadoresPendientes(equipoId);

            if (equipos == null)
                return NoContent();

            return Ok(equipos);
        }

        /// <summary>
        /// Lista agrupadores visibles en app con sus torneos visibles en app (cualquier año).
        /// En torneos cuyo año no es el calendario actual, el nombre incluye el año al final (ej. "Mayores 2024").
        /// Orden de torneos: año calendario actual (A-Z), luego años futuros por año, luego años pasados (más reciente primero).
        /// Los agrupadores sin ningún torneo en ese conjunto no se incluyen en la respuesta.
        /// </summary>
        /// <remarks>
        /// El <see cref="CancellationToken"/> lo inyecta ASP.NET Core: si el cliente corta la petición, se propaga
        /// a EF y la consulta puede cancelarse sin seguir usando la base cuando ya no hay respuesta esperando.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("info-inicial-de-torneos")]
        public async Task<ActionResult<IReadOnlyList<InformacionInicialAgrupadorDTO>>> InformacionInicialDeTorneos(
            CancellationToken cancellationToken)
        {
            var datos = await _core.InformacionInicialDeTorneosAsync(cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Equipos de la zona con datos del club (escudo por URL relativa al mismo criterio que el resto de la API).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("clubes")]
        public async Task<ActionResult<IReadOnlyList<ClubesDTO>>> Clubes([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.ClubesPorZonaAsync(zonaId, cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Fixture todos contra todos de la zona: una entrada por fecha con título "Fecha N" y partidos (local/visitante y escudos).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("fixture-todos-contra-todos")]
        public async Task<ActionResult<FixtureDTO>> FixtureTodosContraTodos([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.FixtureTodosContraTodosAsync(zonaId, cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Jornadas todos contra todos con resultados por categoría del torneo. Una entrada por fecha (título "Fecha N") con un elemento por partido de esa fecha.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("jornadas-todos-contra-todos")]
        public async Task<ActionResult<JornadasDTO>> JornadasTodosContraTodos([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.JornadasTodosContraTodosAsync(zonaId, cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Tabla de posiciones por categoría del torneo (todos contra todos). Posición y puntos pueden ir vacíos hasta definir criterio de desempate.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("posiciones-todos-contra-todos")]
        public async Task<ActionResult<PosicionesDTO>> PosicionesTodosContraTodos([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.PosicionesTodosContraTodosAsync(zonaId, cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Tabla anual: suma de estadísticas de las zonas homónimas en las fases de apertura y clausura del torneo (misma forma que posiciones todos contra todos).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("posiciones-anual")]
        public async Task<ActionResult<PosicionesDTO>> PosicionesAnual([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.PosicionesAnualAsync(zonaId, cancellationToken);
            return Ok(datos);
        }

        /// <summary>
        /// Eliminatorias de la zona: una entrada por fecha (instancia) con día, y un partido por jornada (equipos, escudos y resultado).
        /// </summary>
        [AllowAnonymous]
        [HttpGet("eliminacion-directa")]
        public async Task<ActionResult<EliminacionDirectaDTO>> EliminacionDirecta([FromQuery] int zonaId,
            CancellationToken cancellationToken)
        {
            var datos = await _core.EliminacionDirectaAsync(zonaId, cancellationToken);
            return Ok(datos);
        }
    }
}
