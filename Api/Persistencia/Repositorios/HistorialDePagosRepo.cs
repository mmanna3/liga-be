using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class HistorialDePagosRepo : IHistorialDePagosRepo
{
    private readonly AppDbContext _context;

    public HistorialDePagosRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarPago(int jugadorEquipoId)
    {
        var pago = new HistorialDePagos
        {
            JugadorEquipoId = jugadorEquipoId,
            Fecha = DateTime.Now
        };

        await _context.HistorialDePagos.AddAsync(pago);
    }

    public async Task<IEnumerable<ReportePagosDTO>> ObtenerPagosPorMesYEquipo(int? mes, int? anio)
    {
        var query = _context.HistorialDePagos
            .Include(h => h.JugadorEquipo)
            .ThenInclude(je => je!.Equipo)
            .AsQueryable();

        if (mes.HasValue)
        {
            query = query.Where(h => h.Fecha.Month == mes.Value);
        }

        if (anio.HasValue)
        {
            query = query.Where(h => h.Fecha.Year == anio.Value);
        }

        var resultado = await query
            .GroupBy(h => new { h.Fecha.Month, h.Fecha.Year, h.JugadorEquipo!.Equipo!.Nombre })
            .Select(g => new ReportePagosDTO
            {
                NombreEquipo = g.Key.Nombre,
                Mes = g.Key.Month,
                Anio = g.Key.Year,
                CantidadJugadoresPagados = g.Count()
            })
            .ToListAsync();

        return resultado;
    }
} 