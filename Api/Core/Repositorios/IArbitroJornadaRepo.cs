using Api.Core.Entidades;

namespace Api.Core.Repositorios;

public interface IArbitroJornadaRepo
{
    Task ReemplazarAsignaciones(int jornadaId, IReadOnlyList<(int ArbitroId, int Orden)> asignaciones);
    Task<List<ArbitroJornada>> ListarPorJornadaIds(IEnumerable<int> jornadaIds);
    Task<bool> MarcarWhatsappEnviado(int jornadaId, int arbitroId);
}
