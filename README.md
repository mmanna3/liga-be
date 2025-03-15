# Tools globales que hay que instalar

```
dotnet tool install dotnet-ef -g
```

# BD

## Crear BD local

1- Instalar Docker

2- Descargar la imagen del contenedor
`docker pull mcr.microsoft.com/azure-sql-edge`

3- Creamos el contenedor
`docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=Pas$word!39' -p 1433:1433 --name liga-localhost -d mcr.microsoft.com/azure-sql-edge`

4- Crear BD y aplicar migraciones
`dotnet ef database update`

5- Conectarse desde DataGrip copiando esto en el campo url
`jdbc:sqlserver://localhost:1433;databaseName=liga_dev;user=sa;password=Pas$word!39;encrypt=false;trustServerCertificate=true`

6- Ejecutar el script insertar-datos.sql para insertar datos de prueba en las tablas

## Migraciones

- Agregar: `add-migration NombreDeLaMigracion`
- Borar la última (sin aplicar): `remove-migration`
- Actualizar la BD: `update-database`
- Revertir migraciones aplicadas: `update-database NombreUltimaMigracionBuena`

# Hosting

- Web application firewall: Detection only (si no, no permite PUT y DELETE)

## Alias para migraciones (en .zshrc)

```
alias add-migration="dotnet ef migrations add"
alias update-database="dotnet ef database update"
alias remove-migration="dotnet ef migrations remove"
```

# Agregar ABM

1- En capa Core: crear Entidad, DTO, IRepositorio, IServicio, Core

2- En capa Persistencia: crear Repositorio

3- En capa Api: crear Controller

4- En \_Config: agregar reglas en InyeccinDeDependenciasConfig y MapperConfig


# Crear Usuario
Agregar en la tabla usuario el NombreUsuario y el password obtenerlo así:
```
AuthService.HashPassword("password")
```