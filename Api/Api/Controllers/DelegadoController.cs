using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Api.Controllers
{
    public class DelegadoController : ABMController<DelegadoDTO, IDelegadoCore>
    {
        public DelegadoController(IDelegadoCore core) : base(core)
        {
        }
        
        [HttpPost("blanquear-clave")]
        public async Task<ActionResult<bool>> BlanquearClave(int id)
        {
            return await Core.BlanquearClave(id);
        }
    }
}
