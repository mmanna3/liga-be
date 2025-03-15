using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class JugadorController : ABMController<JugadorDTO, IJugadorCore>
    {
        public JugadorController(IJugadorCore core) : base(core)
        {
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
        public async Task<ActionResult<int>> Activar(CambiarEstadoDelJugadorDTO dto)
        {
            var id = await Core.Activar(dto);
            return Ok(id);
        }
        
        [HttpPost("pagar-fichaje-del-jugador")]
        public async Task<ActionResult<int>> PagarFichaje(CambiarEstadoDelJugadorDTO dto)
        {
            var id = await Core.PagarFichaje(dto);
            return Ok(id);
        }
        
        [HttpPost("inhabilitar-jugador")]
        public async Task<ActionResult<int>> Inhabilitar(CambiarEstadoDelJugadorDTO dto)
        {
            var id = await Core.Inhabilitar(dto);
            return Ok(id);
        }
        
        [HttpPost("suspender-jugador")]
        public async Task<ActionResult<int>> Suspender(CambiarEstadoDelJugadorDTO dto)
        {
            var id = await Core.Suspender(dto);
            return Ok(id);
        }
        
        [HttpGet("listar-con-filtro")]
        public async Task<ActionResult<IEnumerable<JugadorDTO>>> ListarConFiltro([FromQuery] IList<EstadoJugadorEnum> estados)
        {
            var jugadorDTO = await Core.ListarConFiltro(estados);
            return Ok(jugadorDTO);
        }
    }
}
