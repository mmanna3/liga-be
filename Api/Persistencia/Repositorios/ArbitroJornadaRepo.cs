using System.Text.Json;
using Api.Core.DTOs;
using Api.Core.Entidades;
using Api.Core.Repositorios;
using Api.Persistencia._Config;
using Microsoft.EntityFrameworkCore;

namespace Api.Persistencia.Repositorios;

public class ArbitroJornadaRepo : IArbitroJornadaRepo
{
    private readonly AppDbContext _context;

    public ArbitroJornadaRepo(AppDbContext context)
    {
        _context = context;
    }

    public async Task ReemplazarAsignaciones(int jornadaId, IReadOnlyList<(int ArbitroId, int Orden)> asignaciones)
    {
        var existentes = await _context.ArbitroJornada
            .Where(a => a.JornadaId == jornadaId)
            .ToListAsync();
        _context.ArbitroJornada.RemoveRange(existentes);

        foreach (var (arbitroId, orden) in asignaciones)
        {
            _context.ArbitroJornada.Add(new ArbitroJornada
            {
                Id = 0,
                ArbitroId = arbitroId,
                JornadaId = jornadaId,
                Orden = orden,
                WhatsappEnviado = false
            });
        }
    }

    public async Task<bool> MarcarWhatsappEnviado(
        int jornadaId,
        int arbitroId,
        MarcarWhatsappEnviadoArbitroJornadaDTO dto,
        DateTime enviadoEn)
    {
        var asignacion = await _context.ArbitroJornada
            .FirstOrDefaultAsync(a => a.JornadaId == jornadaId && a.ArbitroId == arbitroId);
        if (asignacion == null)
            return false;

        asignacion.WhatsappEnviado = true;
        asignacion.WhatsappHorarioInicio = string.IsNullOrWhiteSpace(dto.HorarioInicio)
            ? null
            : dto.HorarioInicio.Trim();
        asignacion.WhatsappObservaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
            ? null
            : dto.Observaciones.Trim();
        asignacion.WhatsappCategoriasJson = (dto.Categorias ?? []).Count > 0
            ? JsonSerializer.Serialize(dto.Categorias)
            : null;
        asignacion.WhatsappEnviadoEn = enviadoEn;
        return true;
    }

    public Task<List<ArbitroJornada>> ListarPorJornadaIds(IEnumerable<int> jornadaIds)
    {
        var ids = jornadaIds.ToList();
        if (ids.Count == 0)
            return Task.FromResult(new List<ArbitroJornada>());

        return _context.ArbitroJornada
            .Include(a => a.Arbitro)
            .Where(a => ids.Contains(a.JornadaId))
            .ToListAsync();
    }
}
