using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class DelegadoController : ABMController<DelegadoDTO, IDelegadoCore>
    {
        public DelegadoController(IDelegadoCore core) : base(core)
        {
        }
    }
}
