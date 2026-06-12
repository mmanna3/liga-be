using Api.Core.Entidades;
using Api.Core.Otros;

namespace Api.Core.Otros;

public static class ValidadorCategoriasAnual
{
  public const string MensajeSetsDistintos =
      "Las fases de apertura y clausura deben tener las mismas categorías (misma cantidad y nombres en el mismo orden).";

  public static void ValidarSetsIdenticos(
      IReadOnlyList<FaseCategoria> apertura,
      IReadOnlyList<FaseCategoria> clausura)
  {
    var ordenadasA = apertura.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList();
    var ordenadasC = clausura.OrderBy(c => c.Orden).ThenBy(c => c.Id).ToList();

    if (ordenadasA.Count != ordenadasC.Count)
      throw new ExcepcionControlada(MensajeSetsDistintos);

    for (var i = 0; i < ordenadasA.Count; i++)
    {
      if (ordenadasA[i].Nombre.Trim() != ordenadasC[i].Nombre.Trim())
        throw new ExcepcionControlada(MensajeSetsDistintos);
    }
  }
}
