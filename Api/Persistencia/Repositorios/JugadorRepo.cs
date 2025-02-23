using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class JugadorRepo : RepositorioABM<Jugador>, IJugadorRepo
{
    public JugadorRepo(AppDbContext context) : base(context)
    {
    }
}