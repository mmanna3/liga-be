using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class EquipoController : ABMController<EquipoDTO, IEquipoCore, EquipoDTO>
    {
        public EquipoController(IEquipoCore core) : base(core)
        {
        }

        [HttpGet("por-ids", Name = "equiposPorIds")]
        [Authorize(Roles = "Administrador,Consulta")]
        public async Task<ActionResult<IEnumerable<EquipoDTO>>> ObtenerPorIds([FromQuery] IEnumerable<int> ids) =>
            await ObtenerPorIdsCore(ids);

        [HttpGet("{id}/jugadores-que-solo-juegan-en-este-equipo")]
        [Authorize(Roles = "Administrador,Consulta")]
        public async Task<ActionResult<IEnumerable<JugadorBaseDTO>>> JugadoresQueSoloJueganEnEsteEquipo(int id)
        {
            var jugadores = await Core.JugadoresQueSoloJueganEnEsteEquipo(id);
            return Ok(jugadores);
        }

        [HttpGet("equipos-para-zonas", Name = "equiposParaZonas")]
        [Authorize(Roles = "Administrador,Consulta")]
        public async Task<ActionResult<IEnumerable<EquipoParaZonasDTO>>> EquiposParaZonas()
        {
            var equipos = await Core.EquiposParaZonas();
            return Ok(equipos);
        }
    }
}
