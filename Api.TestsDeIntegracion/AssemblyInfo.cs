using Xunit;

// Varios IT comparten rutas bajo bin/.../Imagenes (p. ej. Escudos/1.jpg). En Windows,
// crear/sobrescribir el mismo archivo en paralelo entre clases de test falla con IOException.
// Nombre completo del atributo: existe también el enum Xunit.CollectionBehavior.
[assembly: CollectionBehaviorAttribute(DisableTestParallelization = true)]
