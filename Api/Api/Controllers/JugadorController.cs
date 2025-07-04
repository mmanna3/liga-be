using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoJugador;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class JugadorController : ABMController<JugadorDTO, IJugadorCore>
    {
        public JugadorController(IJugadorCore core) : base(core)
        {
        }
        
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

        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Eliminar(int id)
        {
            var resultado = await Core.Eliminar(id);
            if (resultado == -1)
                return NotFound();
            return Ok(resultado);
        }
        
                
        [HttpPost("desvincular-jugador-del-equipo")]
        public async Task<ActionResult<int>> DesvincularJugadorDelEquipo(DesvincularJugadorDelEquipoDTO dto)
        {
            var id = await Core.DesvincularJugadorDelEquipo(dto);
            return Ok(id);
        }
    }
}
