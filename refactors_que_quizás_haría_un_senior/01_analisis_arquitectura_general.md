# Análisis de Arquitectura General — liga-be

> Revisión de arquitectura como lo haría un Senior/.NET Architect.

---

## Lo que está bien — y bien en serio

**El patrón ABM genérico es el acierto más grande del proyecto.** `ABMController / ABMCore / RepositorioABM` con Template Method (hooks `AntesDeCrear`, `AntesDeModificar`, etc.) elimina boilerplate de forma correcta. No es "magia": cada hook tiene un propósito claro y las subclases saben exactamente dónde enchufar variación. Un controller nuevo funcional en ~15 líneas es un resultado concreto de ese diseño.

**`Core/Logica/` con clases estáticas puras** (`PosicionesTodosContraTodosLogica`, `EliminacionDirectaLogica`) es una decisión de diseño excelente. Separan lógica de dominio compleja sin ninguna dependencia de infraestructura — son triviales de testear y de leer.

**`IRelojZonaHorariaArgentina` para abstraer el tiempo** es el patrón correcto. Muchos proyectos "maduros" tienen lógica de `DateTime.Now` esparcida por todos lados. Acá está correctamente abstraído y mockeable.

**Los atributos de autorización custom** (`AutorizarSoloAdmin`, `AutorizarCualquierUsuario`, etc.) sobre `[Authorize(Roles = "...")]` son una buena decisión ergonómica. Reduce errores de strings hardcodeados y hace el código más expresivo.

**`BackupCore` con ADO.NET raw** para exportación/importación es la decisión correcta. Usar EF Core para bulk operations de backup hubiera sido un error de herramienta. El `OrdenDeRestauracion` array y el `SET IDENTITY_INSERT ON/OFF` muestran comprensión real de SQL Server.

**El test que documenta el workaround de TPH ordering** (`CrearFechasMasivamente_NormalAntesQueInterzonal_LosIdsDeJornadaSiguenElOrdenDelPayload`) es exactamente lo que debería ser un test de integración: documenta una decisión arquitectónica no obvia y protege contra regresiones. Eso es testing como documentación.

**`JornadaSinEquipos` como placeholder explícito** en lugar de nulls. Los slots vacíos del bracket tienen un estado nominal, no son `null`. Esa decisión evita una clase entera de errores de NPE y hace el modelo de dominio más honesto.

**`ExcepcionControlada` + `GlobalExceptionHandler` con `ProblemDetails`** es el estándar correcto para APIs REST modernas en .NET 8. No hay `try/catch` en controllers.

---

## Preguntas — las que haría un arquitecto en code review

### 1. ¿Por qué `AntesDeCrear` recibe tanto el DTO *como* la entidad ya mapeada?

```csharp
protected virtual async Task<TEntidad> AntesDeCrear(TDTO dto, TEntidad entidad)
```

El nombre sugiere "antes de crear", pero en realidad ocurre *después del mapeo*. Si la entidad ya está mapeada, ¿para qué sirve el `dto` en ese punto? ¿Hay casos donde necesitás acceder al DTO porque el mapeo perdió información? Si es así, eso podría indicar que el mapeo está incompleto o que el DTO tiene responsabilidades que deberían estar en la entidad.

### 2. ¿Por qué la distinción `TDTO` / `TCrearDTO` en `ABMController<TDTO, TCore, TCrearDTO>`?

En varios recursos parecen ser el mismo tipo. ¿Hay casos donde realmente difieren? Si la respuesta es "algunos sí, algunos no", vale preguntarse si el tercer parámetro de tipo justifica la complejidad extra, o si habría que resolver eso con inheritance/composition en el DTO mismo.

### 3. ¿Por qué `FechaJsonConverter` con heurística de fallback?

El converter tiene un fallback: si no hay campo `tipo`, infiere el subtipo por presencia de campos (`numero` → TodosContraTodos, `instanciaId` → EliminacionDirecta). ¿Ese fallback existe por compatibilidad con clientes viejos o por conveniencia en desarrollo? Si el cliente siempre manda `tipo`, el fallback es dead code con riesgo de inferencia incorrecta.

**Alternativa disponible en .NET 8:**

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipo")]
[JsonDerivedType(typeof(FechaTodosContraTodosDTO), "TodosContraTodos")]
[JsonDerivedType(typeof(FechaEliminacionDirectaDTO), "EliminacionDirecta")]
public abstract class FechaDTO { }
```

Esto elimina un converter de ~100 líneas, el fallback heurístico, y el mantenimiento manual al agregar subtipos.

### 4. ¿Por qué SQLite para los tests de integración en lugar de SQL Server con Testcontainers?

SQLite tiene diferencias de comportamiento conocidas con SQL Server: sin CHECK constraints reales, sin `IDENTITY_INSERT`, diferente comportamiento de transacciones. Ya tenés código condicional en `AppDbContext` para manejar esto. ¿Eso no es un code smell? ¿El costo de setup de Testcontainers (Docker) no justificaría eliminar esas diferencias?

### 5. ¿Por qué los `Core` services dependen de `IMapper` directamente?

Los `Core/Servicios/` inyectan `IMapper` (AutoMapper). Eso introduce una dependencia de infraestructura en lo que debería ser el núcleo del dominio. ¿Se evaluó mapear en los controllers y que los Cores trabajen solo con entidades? ¿O usar extension methods de mapeo explícitos sin la abstracción de `IMapper`?

### 6. ¿Qué pasa si `ApiKey` está vacío en producción?

`appsettings.json` tiene `ApiKey: ""`. Si olvidás setearlo en producción, `AutorizarConApiKeyAttribute` valida contra string vacío — y cualquier request con header `X-Api-Key: ` pasa. ¿Hay alguna validación en startup?

### 7. `AppCarnetDigitalCore` tiene ~800 líneas y hace carnets, posiciones, planillas y fixture. ¿Es una decisión consciente o fue creciendo?

---

## Observaciones técnicas — lo que cambiaría

### Alta prioridad

**`ExcepcionControlada` no tiene código de error estructurado.**

El cliente frontend tiene que parsear texto para tomar decisiones programáticas. Agregar un `ErrorCode` enum o string constante permitiría:

```csharp
throw new ExcepcionControlada("El jugador está suspendido.", ErrorCode.JugadorSuspendido);
```

Y el cliente reacciona al código, no al texto. Eso también facilita internacionalización futura.

**Ausencia de `CancellationToken` en las interfaces de repositorio.**

```csharp
// Ahora:
Task<T> ObtenerPorId(int id);

// Debería ser:
Task<T> ObtenerPorId(int id, CancellationToken ct = default);
```

Sin `CancellationToken`, si el cliente cancela la request, la operación de base de datos continúa hasta completarse. En CRUD simple el impacto es mínimo. En `BackupCore` (que hace operaciones largas) es relevante.

### Media prioridad

**`AppCarnetDigitalCore` debería dividirse.** Carnets, posiciones y planillas son responsabilidades distintas. Un service de 800 líneas es difícil de leer, testear y modificar. Candidatos de extracción:
- `PosicionesCore` (la lógica de `PosicionesAnualAsync` que mergea apertura/clausura merece tests propios)
- `PlanillaCore`
- `CarnetCore`

**El padding de la clave JWT a 64 bytes en `InyeccionDeDependenciasConfig`** es un workaround para claves cortas en desarrollo. En producción, si la clave tiene menos de 64 bytes, estás firmando con menos entropía de la esperada. Agregar una validación en startup:

```csharp
var token = config["AppSettings:Token"] ?? throw new InvalidOperationException("JWT token key not configured");
if (token.Length < 64) throw new InvalidOperationException("JWT token key must be at least 64 characters");
```

**Los tests de imagen deshabilitan paralelización global** (`DisableTestParallelization = true`) porque comparten paths de sistema de archivos. Lo ideal es que cada clase de test use un directorio temporal único (`Path.GetTempPath() + Guid.NewGuid()`), lo que restaura la paralelización y elimina el side effect compartido.

### Baja prioridad

**`SaveChanges()` por cada jornada en `FechaCore.AplicarJornadasEnFecha`** es el workaround correcto para el bug de TPH ordering, pero genera N roundtrips. Para el volumen típico (8-16 jornadas) es irrelevante. Si escala, vale explorar sequences de SQL Server para controlar el orden de IDs sin depender del orden de insert.

**Los hashes BCrypt del seed están en git history.** Si el repositorio es privado, es aceptable. Si alguna vez se hace público, esos hashes son extraíbles. Para seed data sensible, la alternativa es un script de seed que corre en startup solo si la tabla está vacía, con la clave leída de secrets.

---

## Deuda técnica — tabla resumen

| Prioridad | Item | Esfuerzo |
|-----------|------|----------|
| Alta | Agregar `ErrorCode` estructurado a `ExcepcionControlada` | Bajo |
| Alta | Migrar `FechaJsonConverter` a `[JsonPolymorphic]` nativo de .NET 8 | Medio |
| Media | Dividir `AppCarnetDigitalCore` en servicios más pequeños | Medio |
| Media | Agregar `CancellationToken` a las interfaces de repositorio | Medio |
| Media | Validar en startup que `ApiKey` no esté vacío en producción | Bajo |
| Baja | Revisar si `TCrearDTO` se usa realmente distinto de `TDTO` en todos los recursos | Bajo |
| Baja | Tests de imagen con directorios aislados por clase | Alto |

---

## Síntesis

El proyecto demuestra madurez técnica real. Las decisiones de diseño son mayormente intencionales y bien ejecutadas. Los puntos más débiles son la rigidez del sistema de errores (strings en lugar de códigos), la longitud de `AppCarnetDigitalCore`, y el `FechaJsonConverter` custom que podría reemplazarse con APIs nativas de .NET 8.

Lo más valioso es el diseño de `Core/Logica/` pura, los tests de integración que documentan comportamientos no obvios, y la consistencia del patrón ABM genérico. Un desarrollador nuevo que entre al proyecto puede entender la convención y seguirla sin ambigüedad — eso es raro y vale.
