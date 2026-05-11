# Patrones de Diseño y Unit of Work

---

## 1. Patrones de diseño — ¿dónde refactorizarías?

### Factory Method — duplicado en `TorneoCore`

El mayor problema concreto está en `TorneoCore.cs`. Las funciones `ReemplazarFases` (L80) y `CrearFaseConDatos` (L204) tienen **exactamente el mismo switch expression**:

```csharp
// ReemplazarFases L80 y CrearFaseConDatos L204 — código idéntico copiado
Fase fase = dto.TipoDeFase switch
{
    TipoDeFaseEnum.TodosContraTodos => new FaseTodosContraTodos
        { Id = 0, TorneoId = torneoId, Nombre = dto.Nombre ?? string.Empty, ... },
    TipoDeFaseEnum.EliminacionDirecta => new FaseEliminacionDirecta
        { Id = 0, TorneoId = torneoId, Nombre = dto.Nombre ?? string.Empty, ... },
    _ => throw new ExcepcionControlada("Tipo de fase no válido.")
};
```

**Solución:** un factory method privado `CrearFaseDesdeDTO(int torneoId, FaseDTO dto)`. Elimina duplicación y centraliza el punto de extensión cuando se agregue un tercer tipo de fase.

---

### La misma duplicación afecta a toda la jerarquía TPH

El patrón se repite en `ZonaCore.Crear()`, `ZonaCore.Modificar()`, `FechaCore.Crear()`, `FechaCore.Modificar()`. Cada vez que se agrega un subtipo de `Fase`/`Zona`/`Fecha`, hay que actualizar N switch expressions en N archivos. Un abstract factory o métodos factory por clase base resolvería esto.

---

### Strategy — `Activar`, `Suspender`, `Inhabilitar`

En `JugadorCore.cs`:

```csharp
public async Task<int> Activar(List<CambiarEstadoDelJugadorDTO> dtos)
{
    foreach (var dto in dtos) await CambiarEstado(dto, EstadoJugadorEnum.Activo);
    return dtos.Count;
}
public async Task<int> Suspender(List<CambiarEstadoDelJugadorDTO> dtos) { ... }
public async Task<int> Inhabilitar(List<CambiarEstadoDelJugadorDTO> dtos) { ... }
```

Las tres son idénticas excepto el estado. No hace falta un Strategy formal, pero sí un único método interno `CambiarEstados(dtos, EstadoJugadorEnum estado)` en el core. Los tres métodos públicos pueden delegar en él. El beneficio: el invariante "todos los DTOs se procesan contra el mismo estado" está en un solo lugar.

---

### Observer / Domain Events — side effects acoplados

Hay múltiples operaciones donde el core hace work de BD + side effect de filesystem en un solo método:

- `JugadorCore.AntesDeCrear` (L65–67): guarda en BD + mueve imágenes
- `JugadorCore.Eliminar` (L187–191): elimina de BD + elimina imágenes
- `FechaCore.CargarResultados` (L660–664): guarda resultados + propaga ganadores
- `DelegadoCore.AntesDeCrear`: guarda en BD + mueve imágenes

El problema: si la operación de BD tiene éxito pero la de filesystem falla (o viceversa), el estado queda inconsistente sin posibilidad de rollback. Con **domain events** se separaría la transacción del side effect:

```csharp
// Primero: operación atómica de BD
entidad.RegistrarEvento(new JugadorEliminadoEvent(jugador.DNI));
Repo.Eliminar(jugador);
await BDVirtual.GuardarCambios();

// Después (dispatcher de eventos, fuera de la transacción):
await _dispatcher.Dispatch(entidad.PopEvents());
```

Para este proyecto, el beneficio real es menor porque el filesystem no tiene rollback de todos modos. Pero es la dirección correcta si el sistema escala.

---

### Specification — `PersonaExisteHelper` y validaciones DNI

`JugadorCore.AntesDeCrear` y `DelegadoCore.AntesDeCrear` tienen validaciones casi simétricas sobre si el DNI ya existe como jugador/delegado/pendiente. `PersonaExisteHelper` es ya una proto-especificación, pero hoy las condiciones no son componibles:

```csharp
// Hoy: cada servicio ensambla su propia lógica de validación (repetida)
if (PersonaExisteHelper.JugadorEstaPendiente(jugadorExistente))
    throw ...;
if (PersonaExisteHelper.JugadorExiste(jugadorExistente))
    throw ...;
if (PersonaExisteHelper.DelegadoEstaPendiente(delegadoExistente))
    throw ...;
```

Un `ValidadorDniExistente` con un método `ValidarParaFichar(dni, rol)` centralizaría esas 6-8 líneas que hoy se repiten en ambos cores.

---

## 2. ¿Vale la pena el patrón Unit of Work?

**Respuesta corta: el patrón completo no, pero sí necesitás transacciones explícitas.**

`IBDVirtual` es en realidad la mitad del UoW: la mitad de "commit" (`GuardarCambios`). El `DbContext` de EF Core ya es el UoW completo — trackea cambios en memoria y los commitea atómicamente. Agregar un UoW wrapper sobre EF sería tercera capa de indirección sin ganancia.

### El problema real que el UoW esconde

Mirá `FechaCore.CrearFechaTodosContraTodos`:

```csharp
Repo.Crear(entidad);
await BDVirtual.GuardarCambios();     // Commit 1: persiste la Fecha

await AplicarJornadasEnFecha(id, dto.Jornadas);
await BDVirtual.GuardarCambios();     // Commit 2: persiste Jornadas

await AsegurarPartidosPorCategoriaPorJornada(id, padreId);
await BDVirtual.GuardarCambios();     // Commit 3: persiste Partidos
```

Si el Commit 3 falla, quedan Fecha + Jornadas sin Partidos en la base — estado inválido. La operación lógica debería ser atómica.

### La solución correcta: transacciones explícitas en `IBDVirtual`

No hace falta UoW completo — alcanza con extender la interfaz existente:

```csharp
public interface IBDVirtual
{
    Task GuardarCambios();
    Task<IDbContextTransaction> IniciarTransaccion();
}
```

Así `FechaCore.CrearFechaTodosContraTodos` podría hacer:

```csharp
await using var tx = await BDVirtual.IniciarTransaccion();

Repo.Crear(entidad);
await BDVirtual.GuardarCambios();     // Commit intermedio (necesario por workaround TPH)

await AplicarJornadasEnFecha(id, dto.Jornadas);
await BDVirtual.GuardarCambios();

await AsegurarPartidosPorCategoriaPorJornada(id, padreId);
await BDVirtual.GuardarCambios();

await tx.CommitAsync();               // Todo o nada
```

El caso donde los múltiples `GuardarCambios()` existen **por diseño** (el workaround del TPH ordering de jornadas) es exactamente donde una transacción explícita vale la pena: los commits intermedios son un detalle de implementación, y la transacción garantiza que todo o nada llega a producción.

### Por qué NO implementar UoW completo

El UoW completo también expone los repositorios a través de la misma interfaz:

```csharp
// UoW clásico — NO hacer esto encima de EF Core
public interface IUnitOfWork
{
    IJugadorRepo Jugadores { get; }
    IClubRepo Clubs { get; }
    // ...
    Task GuardarCambios();
}
```

Esto duplica el rol que ya cumple el `DbContext`. EF Core fue diseñado específicamente para ser el UoW. Agregar otra capa es complejidad sin beneficio, y rompe la inyección de dependencias que ya tenés bien organizada.
