using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Api.Controllers;

public class TorneoController : ABMController<TorneoDTO, ITorneoCore>
{
    public TorneoController(ITorneoCore core) : base(core)
    {
    }
} 