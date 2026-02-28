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
    private readonly IDelegadoCore _delegadoCore;

    public PublicoController(IPublicoCore publicoCore, IEquipoCore equipoCore, IDelegadoCore delegadoCore)
    {
        _core = publicoCore;
        _equipoCore = equipoCore;
        _delegadoCore = delegadoCore;
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

        [HttpGet("obtener-club")]
        public async Task<ObtenerNombreEquipoDTO> ObtenerClubPorCodigoAlfanumericoDelEquipo([FromQuery] string codigoAlfanumerico)
        {
            return await _equipoCore.ObtenerClubPorCodigoAlfanumericoDelEquipo(codigoAlfanumerico);
        }

        [HttpGet("obtener-nombre-usuario-disponible")]
        [Produces("application/json")]
        public async Task<string> ObtenerNombreUsuarioDisponible([FromQuery] string nombre, [FromQuery] string apellido)
        {
            return await _delegadoCore.ObtenerNombreUsuarioDisponible(nombre, apellido);
        }

        [HttpPost("fichar-en-otro-equipo")]
        public async Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto)
        {
            return await _core.FicharEnOtroEquipo(dto);
        }
        
        
    }
}
