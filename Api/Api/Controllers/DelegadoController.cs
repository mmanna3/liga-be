using Api.Core.DTOs;
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
            return await base.Crear(dto);
        }

        [HttpPost("blanquear-clave")]
        public async Task<ActionResult<bool>> BlanquearClave(int id)
        {
            return await Core.BlanquearClave(id);
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
