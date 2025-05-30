-- Insertar datos de prueba
DECLARE @Clubes TABLE (Id INT, Nombre NVARCHAR(255));

INSERT INTO Clubs (Nombre)
OUTPUT INSERTED.Id, INSERTED.Nombre INTO @Clubes
VALUES ('Boca'), ('Almirante Brown');

SELECT * FROM @Clubes;

DECLARE @Equipos TABLE (Id INT, Nombre NVARCHAR(255), ClubId INT);

INSERT INTO Equipos (Nombre, ClubId)
OUTPUT INSERTED.Id, INSERTED.Nombre, INSERTED.ClubId INTO @Equipos
SELECT 'Boquita', Id FROM @Clubes WHERE Nombre = 'Boca'
UNION ALL
SELECT 'Bokeh azul', Id FROM @Clubes WHERE Nombre = 'Boca'
UNION ALL
SELECT 'Bokeh amarillo', Id FROM @Clubes WHERE Nombre = 'Boca'
UNION ALL
SELECT 'La fragata', Id FROM @Clubes WHERE Nombre = 'Almirante Brown'
UNION ALL
SELECT 'Aurinegro', Id FROM @Clubes WHERE Nombre = 'Almirante Brown';

-- Primero insertamos los usuarios
DECLARE @Usuarios TABLE (Id INT, NombreUsuario NVARCHAR(14));

INSERT INTO Usuarios (NombreUsuario, Password, RolId)
OUTPUT INSERTED.Id, INSERTED.NombreUsuario INTO @Usuarios
VALUES 
('jperez', '$2a$12$eApHtnPNGVPdlZXTQe7A5uN4eZ9zHjfZcoCKKACdOcQbiGWrLXZI.', 2),
('mgomez', '$2a$12$miPry4RRyPtzE7k1gnmj2Oc/RZxJjsgYk2s9AiqkohOhRjniLiyCG', 2),
('clopez', '$2a$12$eApHtnPNGVPdlZXTQe7A5uN4eZ9zHjfZcoCKKACdOcQbiGWrLXZI.', 2);

-- Luego insertamos los delegados
INSERT INTO Delegados (Nombre, Apellido, ClubId, UsuarioId)
SELECT 'Juan', 'Pérez', Id, (SELECT Id FROM @Usuarios WHERE NombreUsuario = 'jperez') FROM @Clubes WHERE Nombre = 'Boca'
UNION ALL
SELECT 'María', 'Gómez', Id, (SELECT Id FROM @Usuarios WHERE NombreUsuario = 'mgomez') FROM @Clubes WHERE Nombre = 'Boca'
UNION ALL
SELECT 'Carlos', 'López', Id, (SELECT Id FROM @Usuarios WHERE NombreUsuario = 'clopez') FROM @Clubes WHERE Nombre = 'Almirante Brown';

DECLARE @Jugadores TABLE (Id INT, DNI NVARCHAR(18), Nombre NVARCHAR(50), Apellido NVARCHAR(50), FechaNacimiento DATE);

INSERT INTO Jugadores (DNI, Nombre, Apellido, FechaNacimiento)
OUTPUT INSERTED.Id, INSERTED.DNI, INSERTED.Nombre, INSERTED.Apellido, INSERTED.FechaNacimiento INTO @Jugadores
VALUES
('12345678A', 'Martín', 'Palermo', '1973-11-07'),
('87654321B', 'Juan Román', 'Riquelme', '1978-06-24'),
('56789012C', 'Carlos', 'Tevez', '1984-02-05'),
('09876543D', 'Diego', 'Maradona', '1960-10-30'),
('23456789E', 'Hugo', 'Ibarra', '1974-06-01'),
('34567890F', 'Sebastián', 'Battaglia', '1980-11-08'),
('45678901G', 'José', 'Pepe Basualdo', '1963-06-20'),
('56789012H', 'Pascual', 'Ramón', '1975-09-12'),
('67890123I', 'Daniel', 'Bazán Vera', '1981-03-15'),
('78901234J', 'Blas Armando', 'Giunta', '1963-09-06');

INSERT INTO JugadorEquipo (JugadorId, EquipoId, FechaFichaje, EstadoJugadorId, Motivo)
SELECT J.Id, E.Id, '2024-01-15', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '12345678A' AND E.Nombre = 'Boquita'
UNION ALL
SELECT J.Id, E.Id, '2024-02-10', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '87654321B' AND E.Nombre = 'Boquita'
UNION ALL
SELECT J.Id, E.Id, '2024-03-05', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '56789012C' AND E.Nombre = 'Bokeh azul'
UNION ALL
SELECT J.Id, E.Id, '2024-04-20', 2, 'Lesión reciente' FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '09876543D' AND E.Nombre = 'Bokeh azul'
UNION ALL
SELECT J.Id, E.Id, '2024-05-12', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '23456789E' AND E.Nombre = 'Bokeh amarillo'
UNION ALL
SELECT J.Id, E.Id, '2024-06-08', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '34567890F' AND E.Nombre = 'Bokeh amarillo'
UNION ALL
SELECT J.Id, E.Id, '2024-07-01', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '45678901G' AND E.Nombre = 'La fragata'
UNION ALL
SELECT J.Id, E.Id, '2024-08-19', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '56789012H' AND E.Nombre = 'La fragata'
UNION ALL
SELECT J.Id, E.Id, '2024-09-10', 2, 'Falta de documentación' FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '67890123I' AND E.Nombre = 'Aurinegro'
UNION ALL
SELECT J.Id, E.Id, '2024-10-25', 1, NULL FROM @Jugadores J CROSS JOIN @Equipos E WHERE J.DNI = '78901234J' AND E.Nombre = 'Aurinegro';
