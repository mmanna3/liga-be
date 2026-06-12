namespace Api.Core.Otros;

public static class CategoriaPayloadValidador
{
    public static void ValidarOrdenesUnicos(IReadOnlyList<int> ordenes)
    {
        foreach (var orden in ordenes)
        {
            if (orden < 1)
                throw new ExcepcionControlada("El orden de cada categoría debe ser mayor o igual a 1.");
        }

        var duplicados = ordenes.GroupBy(o => o).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicados.Count > 0)
            throw new ExcepcionControlada("No puede haber dos categorías con el mismo orden.");
    }

    public static void ValidarAnios(int anioDesde, int anioHasta)
    {
        if (anioDesde > anioHasta)
            throw new ExcepcionControlada("El año desde no puede ser mayor que el año hasta.");
    }

    public static void ExigirAlMenosUna(IReadOnlyCollection<object> categorias)
    {
        if (categorias.Count == 0)
            throw new ExcepcionControlada("Debe indicar al menos una categoría.");
    }
}
