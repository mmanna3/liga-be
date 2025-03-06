using Api.Core.DTOs;
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
        
        [HttpPost("gestionar-jugador")]
        public async Task<ActionResult<int>> Gestionar(GestionarJugadorDTO dto)
        {
            var jugadorDTO = await Core.Gestionar(dto);
            return Ok(jugadorDTO);
        }
        
        [HttpGet("listar-con-filtro")]
        public async Task<ActionResult<IEnumerable<JugadorDTO>>> ListarConFiltro([FromQuery] IList<EstadoJugadorEnum> estados)
        {
            var jugadorDTO = await Core.ListarConFiltro(estados);
            return Ok(jugadorDTO);
        }
    }
}
