using Api.Core.DTOs;
using Api.Core.DTOs.CambiosDeEstadoDelegado;
using Api.Core.Enums;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class DelegadoController : ABMController<DelegadoDTO, IDelegadoCore>
    {
        public DelegadoController(IDelegadoCore core) : base(core)
        {
        }
        
        [HttpPost]
        [AllowAnonymous]
        public override async Task<ActionResult<DelegadoDTO>> Crear(DelegadoDTO dto)
        {
            var id = await Core.Crear(dto);
            var creado = await Core.ObtenerPorId(id);
            return Ok(creado);
        }

        [HttpPost("aprobar-delegado-en-el-club")]
        public async Task<ActionResult<int>> AprobarDelegadoEnElClub(AprobarDelegadoEnElClubDTO dto)
        {
            var id = await Core.AprobarDelegadoEnElClub(dto.DelegadoClubId);
            if (id == -1)
                return NotFound();
            return Ok(id);
        }

        [HttpPost("blanquear-clave")]
        public async Task<ActionResult<bool>> BlanquearClave(int id)
        {
            return await Core.BlanquearClave(id);
        }

        [HttpGet("listar-delegados-con-filtro")]
        public async Task<ActionResult<IEnumerable<DelegadoDTO>>> ListarConFiltro([FromQuery] IList<EstadoDelegadoEnum> estados)
        {
            var delegadoDTO = await Core.ListarConFiltro(estados);
            return Ok(delegadoDTO);
        }

        [HttpPost("fichar-delegado-solo-con-dni-y-club")]
        [AllowAnonymous]
        public async Task<ActionResult<int>> FicharDelegadoSoloConDniYClub(FicharDelegadoSoloConDniYClubDTO dto)
        {
            var id = await Core.FicharDelegadoSoloConDniYClub(dto);
            return Ok(id);
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> Eliminar(int id)
        {
            var resultado = await Core.Eliminar(id);
            if (resultado == -1)
                return NotFound();
            return Ok(resultado);
        }
    }
}
