using Api.Core.DTOs;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    // TODO: Esto tendría que llamarse FichajeController
    [Route("api/publico")]
    [ApiController]
    [AllowAnonymous]
    public class PublicoController : ControllerBase
    {
        private readonly IPublicoCore _core;
        private readonly IEquipoCore _equipoCore;
        private readonly IDelegadoCore _delegadoCore;
        private readonly IImagenSponsorWebPublicaRepo _imagenSponsorWebPublicaRepo;

        public PublicoController(
            IPublicoCore publicoCore,
            IEquipoCore equipoCore,
            IDelegadoCore delegadoCore,
            IImagenSponsorWebPublicaRepo imagenSponsorWebPublicaRepo)
        {
            _core = publicoCore;
            _equipoCore = equipoCore;
            _delegadoCore = delegadoCore;
            _imagenSponsorWebPublicaRepo = imagenSponsorWebPublicaRepo;
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
        public async Task<ObtenerClubDTO> ObtenerClubPorCodigoAlfanumericoDelEquipo([FromQuery] string codigoAlfanumerico)
        {
            return await _equipoCore.ObtenerClubPorCodigoAlfanumericoDelEquipo(codigoAlfanumerico);
        }

        [HttpGet("obtener-nombre-usuario-disponible")]
        [Produces("application/json")]
        public async Task<string> ObtenerNombreUsuarioDisponible([FromQuery] string nombre, [FromQuery] string apellido)
        {
            return await _delegadoCore.ObtenerNombreUsuarioDisponible(nombre, apellido);
        }

        [HttpGet("obtener-nombre-usuario-por-dni")]
        public async Task<ObtenerNombreUsuarioPorDniDTO> ObtenerNombreUsuarioPorDni([FromQuery] string dni)
        {
            return await _delegadoCore.ObtenerNombreUsuarioPorDni(dni);
        }

        [HttpPost("fichar-en-otro-equipo")]
        public async Task<int> FicharEnOtroEquipo(FicharEnOtroEquipoDTO dto)
        {
            return await _core.FicharEnOtroEquipo(dto);
        }

        // Helper para mí (no usado por usuarios)
        [HttpGet("jugadores-sin-foto")]
        [Produces("text/plain")]
        public async Task<ContentResult> JugadoresSinFoto()
        {
            var texto = await _core.ListarJugadoresSinFoto();
            return Content(texto, "text/plain");
        }

        [HttpGet("escudos-clubes")]
        public async Task<ActionResult<IReadOnlyList<EscudoClubDTO>>> EscudosClubes()
        {
            var escudos = await _core.ListarEscudosDeClubes();
            return Ok(escudos);
        }

        [HttpGet("sponsors-web-publica")]
        public async Task<ActionResult<IReadOnlyList<SponsorWebPublicaPublicoDTO>>> SponsorsWebPublica()
        {
            var sponsors = await _core.ListarSponsorsWebPublica();
            return Ok(sponsors);
        }

        /// <summary>
        /// Sirve el logo del sponsor por ID (evita rutas bloqueadas por ad blockers).
        /// </summary>
        [HttpGet("sponsor-logo/{id:int}")]
        public IActionResult SponsorLogo(int id)
        {
            var path = _imagenSponsorWebPublicaRepo.GetRutaAbsolutaLogo(id);
            if (path is null)
                return NotFound();

            return PhysicalFile(path, "image/jpeg");
        }
    }
}
