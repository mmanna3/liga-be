using Api.Core.DTOs;
using Api.Core.DTOs.AppCarnetDigital;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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
        
        [HttpGet("jugadores-pendientes")]
        public async Task<ActionResult<ICollection<CarnetDigitalDTO>>> JugadoresPendientes(int equipoId)
        {
            var equipos = await _core.JugadoresPendientes(equipoId);

            return Ok(equipos);
        }
    }
}
