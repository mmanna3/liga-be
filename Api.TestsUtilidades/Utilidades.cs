using Api.Core.Entidades;
using Api.Core.Enums;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Api.TestsUtilidades;

public class Utilidades
{
    private readonly AppDbContext _context;

    public Utilidades(AppDbContext context)
    {
        _context = context;
    }
    
    public Club DadoQueExisteElClub()
    {
        var club = new Club
        {
            Nombre = "un club",
            Id = 0
        };
        
        _context.Add(club);
        return club;
    }
}