# Deep link del registro de árbitros: `.well-known` y página que manda al store

## Qué queremos

La gente de la liga les pasa a los árbitros por WhatsApp este link:

```
https://admin.edefi.com.ar/registro-arbitro
```

Si el árbitro tiene la app de EDeFI instalada, el link abre la app directo en el registro de árbitros. Si no la tiene, lo manda al store. Para que funcione, iOS (Universal Links) y Android (App Links) le piden al dominio unos archivos en `/.well-known/` que prueban que el dominio y la app son del mismo dueño. Como `admin.edefi.com.ar` lo sirve este back, los archivos salen de acá.

Alcance: **solo EDeFI**. La parte de la app está en `app-carnet-digital/docs/requerimientos/registro-arbitro-deep-link.md`.

## Antes de empezar: datos que tiene que conseguir un admin

Sin estos datos no se puede terminar. Los tiene que conseguir alguien con acceso de admin a las cuentas de Apple y Google:

1. **Team ID de Apple** (10 caracteres, por ejemplo `AB12CD34EF`):
   1. Entrar a <https://developer.apple.com/account> con la cuenta dueña de la app.
   2. Ir a **Membership details** (o *Detalles de la membresía*).
   3. Copiar el **Team ID**.
2. **ID de la app en el App Store** (un número, para armar el link al store):
   1. Entrar a <https://appstoreconnect.apple.com>.
   2. Ir a **Apps → EDeFI → Información de la app (App Information)**.
   3. Copiar el campo **Apple ID**. El link al store queda `https://apps.apple.com/app/id<ese número>`.
3. **Huella SHA-256 del certificado de firma de Android**:
   1. Entrar a <https://play.google.com/console>.
   2. Ir a **EDeFI → Prueba y lanzamiento → Integridad de la app → Firma de apps** (en inglés: *Test and release → App integrity → App signing*).
   3. Copiar el **SHA-256** de **"Certificado de clave de firma de apps"**. Es el que usa Google para firmar lo que baja la gente del Play Store. **No** es el del keystore local.
   4. Si también querés que el link ande en builds instaladas por fuera del Play Store (por ejemplo un APK de EAS para probar), copiá además el SHA-256 de **"Certificado de clave de subida"**, que está en la misma pantalla. También lo podés ver con `eas credentials -p android`.
4. **Chequear con el hosting que `/.well-known/` llegue a la app.** El deploy (`.github/workflows/deploy.yml`) saltea una carpeta `.well-known` en el servidor, así que ya hay una ahí, probablemente para el certificado SSL. Hay que confirmar que IIS no intercepte `/.well-known/apple-app-site-association` ni `/.well-known/assetlinks.json` y que esos pedidos lleguen a ASP.NET. No hay que tocar lo que ya esté ahí para el SSL.

El link de Play Store no necesita nada del admin: es `https://play.google.com/store/apps/details?id=com.ykn.edefidelegados`.

## Qué hay que hacer

### 1. Config

Una sección nueva en `appsettings.json` (no son secretos):

```json
"DeepLinks": {
  "AppleTeamId": "<Team ID>",
  "IosBundleId": "com.ykn.edefidelegados",
  "AndroidPackage": "com.ykn.edefidelegados",
  "AndroidSha256Fingerprints": ["<SHA-256 firma de apps>", "<SHA-256 subida (opcional)>"],
  "AppStoreUrl": "https://apps.apple.com/app/id<Apple ID>",
  "PlayStoreUrl": "https://play.google.com/store/apps/details?id=com.ykn.edefidelegados"
}
```

Si `DeepLinks` no está configurado (por ejemplo, en otra liga), los endpoints de abajo devuelven 404.

### 2. Endpoints (anónimos)

Servilos desde un controller o con `MapGet`, **no** como archivos en `wwwroot`. `wwwroot` lo pisa el build del front, y `UseStaticFiles` no sirve archivos sin extensión. Tienen que estar antes de `MapFallbackToFile("index.html")`.

1. `GET /.well-known/apple-app-site-association`: devuelve JSON con `Content-Type: application/json`, **sin redirects** (Apple no los sigue):
   ```json
   {
     "applinks": {
       "details": [
         {
           "appIDs": ["<AppleTeamId>.com.ykn.edefidelegados"],
           "components": [{ "/": "/registro-arbitro" }]
         }
       ]
     }
   }
   ```
2. `GET /.well-known/assetlinks.json`: devuelve JSON con `Content-Type: application/json`:
   ```json
   [
     {
       "relation": ["delegate_permission/common.handle_all_urls"],
       "target": {
         "namespace": "android_app",
         "package_name": "com.ykn.edefidelegados",
         "sha256_cert_fingerprints": ["<huellas>"]
       }
     }
   ]
   ```
3. `GET /registro-arbitro`: esto lo ve solo quien **no** tiene la app, porque si la tiene, el sistema operativo abre la app antes de llegar acá. Según el `User-Agent`:
   - iPhone o iPad: `302` a `AppStoreUrl`.
   - Android: `302` a `PlayStoreUrl`.
   - Cualquier otro: un HTML simple con los dos links ("Descargá la app de EDeFI para registrarte como árbitro").

## Tests

Tests de integración en `Api.TestsDeIntegracion`, con el estilo de los que ya existen:

- Los dos `.well-known` devuelven 200, `application/json` y el contenido armado con lo que hay en config (Team ID, package, huellas).
- `/registro-arbitro` redirige al App Store con un User-Agent de iPhone y al Play Store con uno de Android, y devuelve HTML con los dos links en cualquier otro caso.
- Sin la sección `DeepLinks`, los tres devuelven 404.
- Que el fallback del SPA siga andando: una ruta cualquiera del front sigue devolviendo `index.html`.

## Al terminar

Después del deploy, verificalo en producción:

```bash
curl -i https://admin.edefi.com.ar/.well-known/apple-app-site-association
curl -i https://admin.edefi.com.ar/.well-known/assetlinks.json
```

Tienen que dar 200 con el JSON, sin redirects. También se puede validar con <https://developers.google.com/digital-asset-links/tools/generator> (Android) y con <https://branch.io/resources/aasa-validator/> (iOS).
