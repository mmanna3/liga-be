using Api.Core.DTOs;
using Api.Core.Otros;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class ClubController : ABMController<ClubDTO, IClubCore>
    {
        public ClubController(IClubCore core) : base(core)
        {
        }

        [HttpPost("{id}/cambiar-escudo")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<int>> CambiarEscudo(int id, [FromBody] CambiarEscudoDTO dto)
        {
            try
            {
                var resultado = await Core.CambiarEscudo(id, dto.ImagenBase64);
                return Ok(resultado);
            }
            catch (ExcepcionControlada ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
