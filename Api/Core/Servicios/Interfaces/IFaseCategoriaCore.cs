using Api.Core.DTOs;

namespace Api.Core.Servicios.Interfaces;

public interface IFaseCategoriaCore : ICoreABMAnidado<int, FaseCategoriaDTO>
{
    Task ReemplazarCategorias(int faseId, List<FaseCategoriaDTO> categoriasDto, int? torneoIdParaValidarAnual = null);

    Task CopiarDesdePlantillaTorneo(int faseId, int torneoId);

    Task ValidarCategoriasAnualSiAplica(int faseId, int torneoId);
}
