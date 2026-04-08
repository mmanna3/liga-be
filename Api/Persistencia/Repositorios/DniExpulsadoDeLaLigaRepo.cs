using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class DniExpulsadoDeLaLigaRepo : RepositorioABM<DniExpulsadoDeLaLiga>, IDniExpulsadoDeLaLigaRepo
{
    public DniExpulsadoDeLaLigaRepo(AppDbContext context) : base(context)
    {
    }
}
