using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class JugadorController : ABMController<JugadorDTO, IJugadorCore>
    {
        public JugadorController(IJugadorCore core) : base(core)
        {
        }
    }
}
