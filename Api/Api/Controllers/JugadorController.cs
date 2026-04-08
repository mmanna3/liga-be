using Api.Api.Authorization;
using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class JugadorController : ABMController<JugadorDTO, IJugadorCore, JugadorDTO>
    {
        public JugadorController(IJugadorCore core) : base(core)
        {
        }

        [HttpGet("por-ids", Name = "jugadoresPorIds")]
        public async Task<ActionResult<IEnumerable<JugadorDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
            await ObtenerPorIdsCore(ids);

        [HttpPost]
        [AllowAnonymous]
        public override async Task<ActionResult<JugadorDTO>> Crear(JugadorDTO dto)
        {
            return await base.Crear(dto);
        }

        [HttpPost("aprobar-jugador")]
        public async Task<ActionResult<int>> Aprobar(AprobarJugadorDTO dto)
        {
            var id = await Core.Aprobar(dto);
            return Ok(id);
        }

        [HttpPost("rechazar-jugador")]
        public async Task<ActionResult<int>> Rechazar(RechazarJugadorDTO dto)
        {
            var id = await Core.Rechazar(dto);
            return Ok(id);
        }

        [HttpPost("activar-jugador")]
        public async Task<ActionResult<int>> Activar([FromBody] List<CambiarEstadoDelJugadorDTO> dtos)
        {
            var id = await Core.Activar(dtos);
            return Ok(id);
        }

        [HttpPost("efectuar-pases")]
        public async Task<ActionResult<int>> EfectuarPases([FromBody] List<EfectuarPaseDTO> dtos)
        {
            var id = await Core.EfectuarPases(dtos);
            return Ok(id);
        }

        [HttpPost("pagar-fichaje-del-jugador")]
        public async Task<ActionResult<int>> PagarFichaje(CambiarEstadoDelJugadorDTO dto)
        {
            var id = await Core.PagarFichaje(dto);
            return Ok(id);
        }

        [HttpPost("inhabilitar-jugador")]
        public async Task<ActionResult<int>> Inhabilitar([FromBody] List<CambiarEstadoDelJugadorDTO> dtos)
        {
            var id = await Core.Inhabilitar(dtos);
            return Ok(id);
        }

        [HttpPost("suspender-jugador")]
        public async Task<ActionResult<int>> Suspender([FromBody] List<CambiarEstadoDelJugadorDTO> dtos)
        {
            var id = await Core.Suspender(dtos);
            return Ok(id);
        }

        [HttpGet("listar-con-filtro")]
        public async Task<ActionResult<IEnumerable<JugadorDTO>>> ListarConFiltro([FromQuery] IList<EstadoJugadorEnum> estados)
        {
            var jugadorDTO = await Core.ListarConFiltro(estados);
            return Ok(jugadorDTO);
        }

        [HttpPost("desvincular-jugador-del-equipo")]
        public async Task<ActionResult<int>> DesvincularJugadorDelEquipo(DesvincularJugadorDelEquipoDTO dto)
        {
            var id = await Core.DesvincularJugadorDelEquipo(dto);
            return Ok(id);
        }

        [HttpPost("actualizar-tarjetas")]
        public async Task<ActionResult<int>> ActualizarTarjetas(ActualizarTarjetasJugadorDTO dto)
        {
            var id = await Core.ActualizarTarjetas(dto);
            if (id < 0)
                return NotFound();
            return Ok(id);
        }
    }
}
