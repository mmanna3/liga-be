using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers
{
    public class ClubController : ABMController<ClubDTO, IClubCore>
    {
        public ClubController(IClubCore core) : base(core)
        {
        }
    }
}
