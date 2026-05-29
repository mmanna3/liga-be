using Api.Core.DTOs;
using Api.Core.Servicios.Interfaces;

namespace Api.Core.Servicios.Interfaces;

public interface ISponsorWebPublicaCore : ICoreABM<SponsorWebPublicaDTO>
{
    Task<int> CrearConImagen(CrearSponsorWebPublicaDTO dto);
}
