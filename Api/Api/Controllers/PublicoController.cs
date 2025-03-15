using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    [Route("api/publico")]
    [ApiController]
    [AllowAnonymous]
    public class PublicoController : ControllerBase
    {
        private readonly IPublicoCore _core;
        private readonly IEquipoCore _equipoCore;

        public PublicoController(IPublicoCore publicoCore, IEquipoCore equipoCore)
        {
            _core = publicoCore;
            _equipoCore = equipoCore;
        }

        [HttpGet("el-dni-esta-fichado")]
        public async Task<bool> ElDniEstaFichado([FromQuery] string dni)
        {
            return await _core.ElDniEstaFichado(dni);
        }

        [HttpGet("obtener-nombre-equipo")]
        public async Task<ObtenerNombreEquipoDTO> OtenerNombreDelEquipo([FromQuery] string codigoAlfanumerico)
        {
            return await _equipoCore.ObtenerNombrePorCodigoAlfanumerico(codigoAlfanumerico);
        }

        
        
    }
}
