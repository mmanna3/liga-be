using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class EquipoController : ABMController<EquipoDTO, IEquipoCore>
    {
        public EquipoController(IEquipoCore core) : base(core)
        {
        }
    }
}
