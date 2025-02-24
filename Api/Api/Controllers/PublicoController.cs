using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    [Route("api/publico")]
    [ApiController]
    public class PublicoController : ControllerBase
    {
        private readonly IPublicoCore _core;

        public PublicoController(IPublicoCore publicoCore)
        {
            _core = publicoCore;
        }

        public async Task<bool> ElDniEstaFichado(string dni)
        {
            return await _core.ElDniEstaFichado(dni);
        }
    }
}
