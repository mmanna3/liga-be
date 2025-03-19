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
        [HttpGet("equipos-del-delegado")]
        public async Task<ActionResult<string>> Equipos()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var username = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                if (username != null)
                {
                    return Ok(username);
                }
            }

            return Unauthorized("No se pudo obtener el usuario del token.");
        }
    }
}
