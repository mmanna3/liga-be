using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Logica;
using Api.Core.Otros;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class EquipoCore : ABMCore<IEquipoRepo, Equipo, EquipoDTO>, IEquipoCore
{
    public EquipoCore(IBDVirtual bd, IEquipoRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }

    public async Task<ObtenerNombreEquipoDTO> ObtenerNombrePorCodigoAlfanumerico(string codigoAlfanumerico)
    {
        int id;
        try
        {
            id = GeneradorDeHash.ObtenerSemillaAPartirDeAlfanumerico7Digitos(codigoAlfanumerico);
        }
        catch (ExcepcionControlada e)
        {
            return ObtenerNombreEquipoDTO.Error(e.Message);
        }
        
        var equipo = await Repo.ObtenerPorId(id);
        if (equipo == null)
            return ObtenerNombreEquipoDTO.Error("El código alfanumérico no pertenece a ningún equipo.");
        
        return ObtenerNombreEquipoDTO.Exito(equipo.Nombre);
    }
}