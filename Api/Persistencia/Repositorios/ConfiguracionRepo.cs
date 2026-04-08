using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;

namespace Api.Persistencia.Repositorios;

public class ConfiguracionRepo : RepositorioABM<Configuracion>, IConfiguracionRepo
{
    public ConfiguracionRepo(AppDbContext context) : base(context)
    {
    }
}
