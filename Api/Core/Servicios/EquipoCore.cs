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

    protected override async Task<Equipo> AntesDeCrear(EquipoDTO dto, Equipo entidad)
    {
        // Verificar si ya existe un equipo con el mismo nombre en el mismo torneo
        if (await Repo.ExisteEquipoConMismoNombreEnTorneo(entidad.Nombre, entidad.TorneoId))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }
        
        return await base.AntesDeCrear(dto, entidad);
    }

    protected override async Task<Equipo> AntesDeModificar(int id, EquipoDTO dto, Equipo entidadAnterior, Equipo entidadNueva)
    {
        // Si el nombre o el torneo cambiaron, verificar que no exista otro equipo con el mismo nombre en el mismo torneo
        if ((entidadAnterior.Nombre != entidadNueva.Nombre || entidadAnterior.TorneoId != entidadNueva.TorneoId) && 
            await Repo.ExisteEquipoConMismoNombreEnTorneo(entidadNueva.Nombre, entidadNueva.TorneoId, id))
        {
            throw new ExcepcionControlada("Ya existe un equipo con el mismo nombre en este torneo.");
        }

        return await base.AntesDeModificar(id, dto, entidadAnterior, entidadNueva);
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

    public async Task<ObtenerNombreEquipoDTO> ObtenerClubPorCodigoAlfanumericoDelEquipo(string codigoAlfanumerico)
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

        var club = equipo.Club;
        if (club == null)
            return ObtenerNombreEquipoDTO.Error("El equipo no tiene club asociado.");

        return ObtenerNombreEquipoDTO.Exito(club.Nombre);
    }
}