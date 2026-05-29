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
        var pagoExistente = await _context.HistorialDePagos
            .FirstOrDefaultAsync(h => h.JugadorEquipoId == jugadorEquipoId);
        
        if (pagoExistente != null)
        {
            // Si ya existe un pago, actualizamos la fecha
            pagoExistente.Fecha = DateTime.Now;
            _context.HistorialDePagos.Update(pagoExistente);
        }
        else
        {
            // Si no existe, creamos uno nuevo
            var pago = new HistorialDePagos
            {
                JugadorEquipoId = jugadorEquipoId,
                Fecha = DateTime.Now
            };

            await _context.HistorialDePagos.AddAsync(pago);
        }
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

    public async Task<IEnumerable<ReporteJugadoresHabilitadosPorTorneoDTO>> ObtenerJugadoresHabilitadosPorTorneo(int anio)
    {
        var query =
            from h in _context.HistorialDePagos
            join je in _context.JugadorEquipo on h.JugadorEquipoId equals je.Id
            join ez in _context.EquipoZona on je.EquipoId equals ez.EquipoId
            join z in _context.Zonas on ez.ZonaId equals z.Id
            join f in _context.Fases on z.FaseId equals f.Id
            join t in _context.Torneos on f.TorneoId equals t.Id
            where h.Fecha.Year == anio
            select new { h, t };

        var agrupadoPorTorneoYMes = await query
            .GroupBy(x => new
            {
                x.t.Nombre,
                x.h.Fecha.Month
            })
            .Select(g => new
            {
                g.Key.Nombre,
                g.Key.Month,
                Cantidad = g.Select(x => x.h.JugadorEquipoId).Distinct().Count()
            })
            .ToListAsync();

        var resultado = agrupadoPorTorneoYMes
            .GroupBy(x => x.Nombre)
            .Select(g =>
            {
                var meses = g.ToDictionary(x => x.Month, x => x.Cantidad);
                return new ReporteJugadoresHabilitadosPorTorneoDTO
                {
                    NombreTorneo = g.Key,
                    Enero = meses.GetValueOrDefault(1),
                    Febrero = meses.GetValueOrDefault(2),
                    Marzo = meses.GetValueOrDefault(3),
                    Abril = meses.GetValueOrDefault(4),
                    Mayo = meses.GetValueOrDefault(5),
                    Junio = meses.GetValueOrDefault(6),
                    Julio = meses.GetValueOrDefault(7),
                    Agosto = meses.GetValueOrDefault(8),
                    Septiembre = meses.GetValueOrDefault(9),
                    Octubre = meses.GetValueOrDefault(10),
                    Noviembre = meses.GetValueOrDefault(11),
                    Diciembre = meses.GetValueOrDefault(12),
                    TotalEnElAnio = g.Sum(x => x.Cantidad)
                };
            })
            .OrderBy(x => x.NombreTorneo)
            .ToList();

        return resultado;
    }

    public HistorialDePagos? ObtenerPagoPorJugadorEquipoId(int jugadorEquipoId)
    {
        return _context.HistorialDePagos
            .FirstOrDefault(h => h.JugadorEquipoId == jugadorEquipoId);
    }
} 