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
    
    public Club? DadoQueExisteElClub()
    {
        var club = new Club
        {
            Nombre = "un club",
            Id = 0
        };
        
        _context.Add(club);
        return club;
    }
    
    public Equipo? DadoQueExisteElEquipo(Club? club)
    {
        var equipo = new Equipo
        {
            Nombre = "un equipo",
            Id = 0,
            Club = club,
            ClubId = club?.Id ?? 0,
            Jugadores = new List<JugadorEquipo>()
        };
        
        _context.Add(equipo);
        return equipo;
    }
}