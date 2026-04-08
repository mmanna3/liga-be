using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Core.Servicios.Interfaces;
using AutoMapper;

namespace Api.Core.Servicios;

public class DniExpulsadoDeLaLigaCore : ABMCore<IDniExpulsadoDeLaLigaRepo, DniExpulsadoDeLaLiga, DniExpulsadoDeLaLigaDTO>,
    IDniExpulsadoDeLaLigaCore
{
    public DniExpulsadoDeLaLigaCore(IBDVirtual bd, IDniExpulsadoDeLaLigaRepo repo, IMapper mapper) : base(bd, repo, mapper)
    {
    }
}
