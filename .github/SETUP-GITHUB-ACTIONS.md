# Configuración de GitHub Actions para Liga-BE

Este documento explica cómo configurar los secrets en GitHub para que el workflow de CI/CD funcione tras migrar desde AppVeyor.

## Arquitectura multi-cliente (Environments)

El workflow usa **GitHub Environments** para soportar múltiples clientes. Cada cliente tiene su propio entorno con sus secrets, permitiendo:

- Mismo código, distintas bases de datos y servidores por cliente
- Escalar añadiendo nuevos environments
- Deploy en paralelo de todos los clientes
- `fail-fast: false`: si falla un cliente, los demás siguen desplegando

## 1. Crear Environments por cliente

1. Ve al repositorio en GitHub.
2. **Settings** → **Environments**.
3. Crea un environment por cliente (ej: `cliente1`, `cliente2`, `production-acme`).
4. Actualiza la matriz en `deploy.yml` para incluir cada environment:

```yaml
strategy:
  matrix:
    environment: [cliente1, cliente2]  # Añade aquí cada cliente
```

## 2. Secrets por Environment

Cada environment debe tener **sus propios secrets**. Los nombres son los mismos en todos; el valor cambia por cliente.

1. En **Settings** → **Environments**, haz clic en el environment (ej: `cliente1`).
2. En **Environment secrets**, añade los siguientes:

| Secret | Descripción |
|--------|-------------|
| `DB_SERVER` | Servidor de SQL Server (ej: `sql.ejemplo.com` o `IP,1433`) |
| `DB_NAME` | Nombre de la base de datos |
| `DB_USERNAME` | Usuario de la base de datos |
| `DB_PASSWORD` | Contraseña de la base de datos |
| `CLAVE_SECRETA_JWT` | Clave para firmar tokens JWT (mín. 64 caracteres) |
| `DEPLOY_USERNAME` | Usuario para Web Deploy en los servidores IIS |
| `DEPLOY_PASSWORD` | Contraseña para Web Deploy |
| `DEPLOY_PUBLISH_URL` | URL del servicio Web Deploy del servidor |
| `MS_DEPLOY_SITE` | Nombre del sitio IIS en el servidor|

Repite para cada environment/cliente.

## 3. Añadir un nuevo cliente

1. **Settings** → **Environments** → **New environment** (ej: `cliente3`).
2. Añade los secrets del cliente.
3. Edita `deploy.yml` y añade el environment a la matriz:

```yaml
matrix:
  environment: [cliente1, cliente2, cliente3]
```

## 4. Formato de las URLs de Web Deploy

`DEPLOY_PUBLISH_URL_1` y `DEPLOY_PUBLISH_URL_2` suelen tener un formato como:

- `https://tudominio.com:8172`  
  o  
- `https://tudominio.com:8172/msdeploy.axd`

Si el deploy falla por conexión, prueba con la otra variante. El puerto 8172 es el habitual para el servicio Web Deploy.

## 5. Comprobar que funciona

1. Haz un push a la rama `main`.
2. Ve a la pestaña **Actions** del repo.
3. Deberías ver un job por cliente (ej: `build-and-deploy (cliente1)`, `build-and-deploy (cliente2)`).
4. Revisa los logs si algo falla.
