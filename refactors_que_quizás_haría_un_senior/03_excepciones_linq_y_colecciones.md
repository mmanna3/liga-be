# Excepciones, LINQ y Tipos de Colecciones

---

## 3. Manejo de excepciones — mejoras concretas

### Problema 1: `-1` como señal de "no encontrado"

```csharp
// ABMCore.Eliminar
var entidad = await Repo.ObtenerPorId(id);
if (entidad == null)
    return -1;  // ← señal implícita que el controller interpreta
```

```csharp
// ABMController.Eliminar
var resultado = await Core.Eliminar(id);
if (resultado == -1)
    return NotFound();
```

Esto funciona, pero mezcla el protocolo de error en el tipo de retorno. ¿Qué pasa si en el futuro un Core devuelve -1 por otra razón? El mismo patrón se repite en `Aprobar`, `Rechazar`, `PagarFichaje` en `JugadorCore`, pero ahí el controller **no chequea** el -1 — es un error silencioso.

**La solución correcta:** una excepción semántica específica:

```csharp
public class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string resource, int id)
        : base($"{resource} con id {id} no existe.") { }
}
```

En `GlobalExceptionHandler`:

```csharp
var statusCode = exception switch
{
    ExcepcionControlada => 400,
    RecursoNoEncontradoException => 404,
    _ => 500
};
```

El controller quedaría limpio:

```csharp
public async Task<ActionResult<int>> Eliminar(int id)
{
    return Ok(await Core.Eliminar(id));
}
```

Y en `ABMCore`:

```csharp
var entidad = await Repo.ObtenerPorId(id)
    ?? throw new RecursoNoEncontradoException(typeof(TEntidad).Name, id);
```

---

### Problema 2: `if (jugador != null) { ... } return -1` en Aprobar/Rechazar/PagarFichaje

```csharp
// JugadorCore.Aprobar L195
if (jugador != null) { 
    // ... lógica ...
    return dto.JugadorEquipoId;
}
return -1;
```

El `-1` no se loguea, no tiene mensaje, y el caller (el controller) tampoco lo chequea. En la práctica es un error silencioso. Debería ser:

```csharp
var jugador = await Repo.ObtenerPorId(dto.Id)
    ?? throw new RecursoNoEncontradoException("Jugador", dto.Id);
```

---

### Problema 3: `DbUpdateConcurrencyException` capturado en el controller

```csharp
// ABMController.Put L77
catch (Exception e)
{
    if (e is DbUpdateConcurrencyException)
        return NotFound();
    throw;
}
```

Esta es la única excepción de infraestructura capturada en el controller. Viola el principio de que `GlobalExceptionHandler` es el único punto de manejo. Además, `DbUpdateConcurrencyException` → 404 es semánticamente incorrecto: el recurso existe, hay un conflicto de versión → debería ser 409 Conflict.

**Solución:** moverla al handler global:

```csharp
// GlobalExceptionHandler
var statusCode = exception switch
{
    ExcepcionControlada => 400,
    RecursoNoEncontradoException => 404,
    DbUpdateConcurrencyException => 409,
    _ => 500
};
```

---

### Problema 4: sin error codes estructurados

Con el código real se ve claramente en `JugadorCore`:

```csharp
throw new ExcepcionControlada("El jugador ya juega en otro equipo del mismo torneo.");
throw new ExcepcionControlada("El jugador ya está fichado en el equipo");
throw new ExcepcionControlada("DNI ya existente en el sistema. Probá ficharte desde el flujo 'Solo con DNI'.");
```

El frontend tiene que parsear texto para distinguir estos casos. Un `ErrorCode` enum o string constante resolvería esto:

```csharp
public class ExcepcionControlada : Exception
{
    public string? ErrorCode { get; }

    public ExcepcionControlada(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
```

Uso:

```csharp
throw new ExcepcionControlada(
    "El jugador ya juega en otro equipo del mismo torneo.",
    "JUGADOR_YA_EN_TORNEO");
```

Y el `ProblemDetails` que devuelve el handler puede incluir el `ErrorCode` como extension property.

---

## 4. LINQ y tipos de colecciones

### `IEnumerable<T>` desde async = semántica incorrecta

```csharp
// ICoreABM.cs
Task<IEnumerable<TDTO>> Listar();

// IRepositorioABM.cs
Task<IEnumerable<TModel>> Listar();
```

El resultado de `ToListAsync()` es siempre una `List<T>` ya materializada. Al devolverla como `IEnumerable<T>`, el contrato dice "esto podría ser lazy" cuando en realidad no lo es. El caller puede llamar `.ToList()` innecesariamente, o asumir que puede iterarlo múltiples veces sin costo (en el caso de un `IQueryable` encubierto, esto sería un bug).

**El tipo correcto es `Task<IReadOnlyList<T>>`** — materializado, inmutable desde afuera, indexable.

---

### `ICollection<T>` expone mutación innecesaria

```csharp
// IAppCarnetDigitalCore.cs
Task<ICollection<CarnetDigitalDTO>?> Carnets(int equipoId);
Task<ICollection<CarnetDigitalPendienteDTO>?> JugadoresPendientes(int equipoId);
```

`ICollection<T>` tiene `Add`, `Remove`, `Clear`. El caller de `Carnets` no debería poder mutar la lista devuelta. `IReadOnlyList<T>` es el tipo correcto — el caller puede iterar e indexar, pero no modificar.

---

### Parámetros de escritura donde solo se lee

```csharp
// IJugadorCore.cs
Task<IEnumerable<JugadorDTO>> ListarConFiltro(IList<EstadoJugadorEnum> estados);
Task<int> Activar(List<CambiarEstadoDelJugadorDTO> dtos);
Task<int> Suspender(List<CambiarEstadoDelJugadorDTO> dtos);
```

`IList<T>` tiene `Insert`, `RemoveAt`. `List<T>` en Activar/Suspender tiene `Sort`, `Add`, etc. El core nunca muta estos parámetros. Deberían recibir `IReadOnlyList<T>`. Esto también desacopla al caller de tener que pasar exactamente una `List<T>`.

---

### N+1 en `ObtenerPorId(IEnumerable<int>)`

```csharp
// JugadorCore.cs L84
public override async Task<IEnumerable<JugadorDTO>> ObtenerPorId(IEnumerable<int> ids)
{
    var dtos = (await base.ObtenerPorId(ids)).ToList();
    foreach (var dto in dtos)
        dto.DelegadoId = await Repo.ObtenerDelegadoIdPorDNI(dto.DNI);  // N queries a la BD
    return dtos;
}
```

Igual en `DelegadoCore`. Para N jugadores, hace N queries adicionales a la BD. Debería ser un solo JOIN:

```csharp
var dnis = dtos.Select(d => d.DNI).ToList();
var delegadoIds = await Repo.ObtenerDelegadoIdsPorDNIs(dnis);  // 1 query con WHERE IN
foreach (var dto in dtos)
    dto.DelegadoId = delegadoIds.GetValueOrDefault(dto.DNI);
```

---

### Mutación de entidades trackeadas en `ListarConFiltro`

```csharp
// JugadorCore.cs L167
var jugadorConUnSoloEquipo = jugEquipo.Jugador;
jugadorConUnSoloEquipo.JugadorEquipos = new List<JugadorEquipo> { jugEquipo };  // ← mutación
var dto = Mapper.Map<JugadorDTO>(jugadorConUnSoloEquipo);
```

`jugEquipo.Jugador` es una entidad rastreada por EF Core. Mutar `JugadorEquipos` sobre una entidad trackeada es una bomba de tiempo — si hubiera un `SaveChanges` posterior en el mismo request, podría persistir el cambio. La solución es proyectar directamente al DTO sin mutar la entidad.

---

### `QuitarCaracteresNoNumericos` duplicado

```csharp
// JugadorCore.cs L73
private static string QuitarCaracteresNoNumericos(string dni)
    => new string(dni.Where(char.IsDigit).ToArray());

// DelegadoCore.cs L35  
private static string QuitarCaracteresNoNumericos(string dni) 
    => new string(dni.Where(char.IsDigit).ToArray());
```

Exactamente el mismo método privado en dos clases. Debería vivir en `Core/Logica/` como método utilitario compartido.

---

### `FirstOrDefault` dentro de N loops en `CalcularDatosPosicionesZonaAsync`

```csharp
// AppCarnetDigitalCore.cs
foreach (var cat in categorias)
{
    foreach (var equipo in equipos)
    {
        foreach (var fecha in fechas)
        {
            foreach (var j in fecha.Jornadas)
            {
                var partido = j.Partidos?.FirstOrDefault(p => p.CategoriaId == cat.Id);
                // ↑ escaneo lineal dentro del loop más interior, repetido por cada (cat, equipo)
```

Esta búsqueda ocurre `categorias × equipos` veces por cada jornada. Si hay 5 categorías y 8 equipos, se escanea la lista `Partidos` 40 veces por jornada.

**Solución:** buildear un `Dictionary` una sola vez por jornada, afuera del loop de categorías:

```csharp
foreach (var fecha in fechas)
{
    foreach (var j in fecha.Jornadas)
    {
        var partidosPorCategoria = j.Partidos?.ToDictionary(p => p.CategoriaId) ?? [];
        
        foreach (var cat in categorias)
        {
            if (!partidosPorCategoria.TryGetValue(cat.Id, out var partido))
                continue;
            // ... el resto de la lógica
        }
    }
}
```

Esto cambia la complejidad del inner loop de O(categorias × equipos × partidos_por_jornada) a O(categorias × equipos).

---

### Doble `GuardarCambios` en `EfectuarPases`

```csharp
// JugadorCore.cs L283-287
foreach (var dto in dtos)
{
    // ...
    await BDVirtual.GuardarCambios();  // dentro del loop
}
await BDVirtual.GuardarCambios();      // después del loop — redundante
```

El `GuardarCambios` dentro del loop ya commitea cada iteración. El del final es redundante. Si la intención era "hacer un solo commit al final", entonces el del loop está de más. Hay que decidir cuál es la semántica correcta y dejar solo uno.

---

### `EquipoDeLaZonaDTO.Id` como `string` — fuerza parsing innecesario

```csharp
// ZonaCore.cs L183
private static List<int> ParsearEquipoIds(List<EquipoDeLaZonaDTO> equipos)
{
    var ids = new List<int>();
    foreach (var e in equipos)
    {
        if (int.TryParse(e.Id, out var equipoId))
            ids.Add(equipoId);
    }
    return ids;
}
```

¿Por qué `Id` es `string` si siempre es un entero? Si la respuesta es "porque el cliente manda strings", el fix está en el DTO con `[JsonConverter]` o type binding, no en el Core. Hoy un ID inválido se ignora silenciosamente en el `if (int.TryParse(...))` — podría ser un bug oculto donde una zona se crea sin equipos sin que nadie se entere.

---

## Tabla resumen — colecciones

| Ubicación | Tipo actual | Tipo correcto | Motivo |
|-----------|-------------|---------------|--------|
| `ICoreABM.Listar()` | `Task<IEnumerable<T>>` | `Task<IReadOnlyList<T>>` | Dato ya materializado |
| `IRepositorioABM.Listar()` | `Task<IEnumerable<T>>` | `Task<IReadOnlyList<T>>` | Ídem |
| `IAppCarnetDigitalCore.Carnets()` | `Task<ICollection<T>?>` | `Task<IReadOnlyList<T>?>` | No exponer mutación |
| `IJugadorCore.ListarConFiltro()` | parámetro `IList<T>` | `IReadOnlyList<T>` | El core no muta |
| `IJugadorCore.Activar/Suspender/Inhabilitar()` | parámetro `List<T>` | `IReadOnlyList<T>` | Ídem |
