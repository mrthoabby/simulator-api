## API de Autenticación (Auth)

Esta documentación describe **solo** el módulo de autenticación de la Product Management System API:

- Cómo iniciar sesión y obtener tokens.
- Cómo refrescar el access token.
- Cómo cerrar sesión (una sesión o todas).
- Cómo validar un token.
- Endpoints pensados para administración (revocar tokens de un usuario, limpiar expirados).

La API usa **JWT Bearer** para autenticación.

---

## Configuración necesaria (resumen)

En `appsettings*.json` o variables de entorno deben estar configurados:

- **MongoDB**
  - `MongoDbSettings:ConnectionString`
  - `MongoDbSettings:DatabaseName`
- **JWT**
  - `JwtSettings:SecretKey`
  - `JwtSettings:Issuer`
  - `JwtSettings:Audience`
- **Seguridad**
  - `SecuritySettings:PasswordPepper`
- **Auth**
  - `AuthSettings` (controla si la autenticación está habilitada, etc.).

En desarrollo, puedes arrancar la API con:

```bash
cd ProductManagementSystem.Application
dotnet run
```

Por defecto, en **Development**, tendrás **Swagger UI** en la raíz:

- Swagger UI: `https://localhost:{PORT}/`
- Swagger JSON: `https://localhost:{PORT}/swagger/v1/swagger.json`

---

## Rutas base del módulo Auth

Controlador: `AuthController`

Ruta base:

```text
api/Auth
```

Endpoints:

- `POST api/Auth/login`
- `POST api/Auth/refresh`
- `POST api/Auth/logout`
- `POST api/Auth/validate`
- `POST api/Auth/revoke/{userId}`  (admin)
- `POST api/Auth/cleanup`          (admin)

---

## DTOs principales

### LoginDTO (request de `/login`)

```json
{
  "email": "user@example.com",
  "password": "MyPassword123!",
  "device_id_to_revoke": "OPCIONAL: id_de_sesion_a_cerrar"
}
```

- **email**: string, requerido, email válido.
- **password**: string, requerido.
- **device_id_to_revoke**: string?, opcional.
  - Solo se usa cuando se ha superado el límite de sesiones.
  - Debe ser el **Token** (access token) de una sesión activa que se quiere cerrar.

---

### RefreshTokenDTO (request de `/refresh`)

```json
{
  "refresh_token": "string-largo-del-refresh-token"
}
```

---

### LogoutDTO (request de `/logout`)

```json
{
  "refresh_token": "opcional-si-quieres-solo-esa-sesion",
  "logout_all_devices": true
}
```

- **refresh_token**: si se informa y `logout_all_devices = false`, se cierra solo esa sesión.
- **logout_all_devices**: si es `true`, se revocan todas las sesiones del usuario actual.

---

### AuthResponseDTO (respuesta de `/login` y `/refresh`)

Ejemplo completo:

```json
{
  "user": {
    "id": "USER_ID",
    "email": "user@example.com",
    "name": "Demo User",
    "roles": ["User"]
  },
  "access_token": {
    "token": "jwt-access-token...",
    "type": "Bearer",
    "expires_at": "2025-01-01T12:00:00Z",
    "created_at": "2025-01-01T11:00:00Z"
  },
  "refresh_token": {
    "token": "random-refresh-token...",
    "type": "refresh",
    "expires_at": "2025-02-01T11:00:00Z",
    "created_at": "2025-01-01T11:00:00Z"
  },
  "authenticated_at": "2025-01-01T11:00:00Z",
  "expires_in": 3600
}
```

- `expires_in` = segundos hasta que expire el access token.

---

## Cómo consumir el módulo Auth

En los ejemplos de abajo asume:

- Base URL: `https://localhost:5001/` (ajusta el puerto según tu entorno).

---

### 1. Login (obtener access y refresh token)

- **Método**: `POST`
- **Ruta**: `api/Auth/login`
- **Body**: `LoginDTO`
- **Auth**: No requiere `Authorization` header (es el login).

#### Request ejemplo (cURL)

```bash
curl -X POST "https://localhost:5001/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "MyPassword123!"
  }'
```

#### Respuesta esperada (200 OK)

```json
{
  "user": {
    "id": "64f1...",
    "email": "user@example.com",
    "name": "Demo User",
    "roles": ["User"]
  },
  "access_token": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "type": "Bearer",
    "expires_at": "2025-01-01T12:00:00Z",
    "created_at": "2025-01-01T11:00:00Z"
  },
  "refresh_token": {
    "token": "random-refresh-token...",
    "type": "refresh",
    "expires_at": "2025-02-01T11:00:00Z",
    "created_at": "2025-01-01T11:00:00Z"
  },
  "authenticated_at": "2025-01-01T11:00:00Z",
  "expires_in": 3600
}
```

#### Errores típicos

- `400 BadRequest`: problemas de validación del body.
- `401 Unauthorized`: credenciales incorrectas.
- `500 InternalServerError`: error interno.

#### Caso especial: límite de dispositivos/sesiones

Si el usuario superó el número máximo de sesiones activas definido por sus **planes de usuario**, el servicio:

- Lanza una excepción tipo `DeviceLimitExceededException` con información de las sesiones activas.
- El cliente debe:
  1. Mostrar al usuario la lista de sesiones/dispositivos activos.
  2. Elegir uno para cerrar.
  3. Reintentar el login incluyendo `device_id_to_revoke`.

##### Ejemplo de segundo intento de login con `device_id_to_revoke`

```bash
curl -X POST "https://localhost:5001/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "MyPassword123!",
    "device_id_to_revoke": "ACCESS_TOKEN_A_CERRAR"
  }'
```

---

### 2. Refresh token (obtener nuevos tokens)

- **Método**: `POST`
- **Ruta**: `api/Auth/refresh`
- **Body**: `RefreshTokenDTO`
- **Auth**: No requiere header `Authorization`, el refresh va en el body.

#### Request ejemplo

```bash
curl -X POST "https://localhost:5001/api/Auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refresh_token": "random-refresh-token..."
  }'
```

#### Comportamiento interno

- Busca el refresh token en Mongo (`auth_tokens`).
- Debe:
  - Existir.
  - Ser de tipo `Refresh`.
  - No estar revocado.
  - No estar expirado.
- Revoca el refresh token antiguo.
- Genera un nuevo par **access + refresh** y los persiste.
- Devuelve un nuevo `AuthResponseDTO` con los tokens actualizados.

#### Respuesta ejemplo (200 OK)

Mismo formato que en el login:

```json
{
  "user": { "...": "..." },
  "access_token": { "...": "..." },
  "refresh_token": { "...": "..." },
  "authenticated_at": "2025-01-01T11:30:00Z",
  "expires_in": 3600
}
```

---

### 3. Logout (cerrar sesión)

- **Método**: `POST`
- **Ruta**: `api/Auth/logout`
- **Body**: `LogoutDTO`
- **Auth**: Requiere `Authorization: Bearer {accessToken}`.

#### Cerrar todas las sesiones del usuario actual

```bash
curl -X POST "https://localhost:5001/api/Auth/logout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOi..." \
  -d '{
    "logout_all_devices": true
  }'
```

- El servicio revoca **todos** los tokens asociados al usuario actual.
- Respuesta: `204 NoContent`.

#### Cerrar solo una sesión concreta (por refresh token)

```bash
curl -X POST "https://localhost:5001/api/Auth/logout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOi..." \
  -d '{
    "refresh_token": "random-refresh-token...",
    "logout_all_devices": false
  }'
```

- Revoca solo el refresh token indicado (si está activo).
- Respuesta: `204 NoContent`.

---

### 4. Validar un access token

- **Método**: `POST`
- **Ruta**: `api/Auth/validate`
- **Body**: un string JSON con el token.
- **Auth**: No requiere header `Authorization`.

#### Request ejemplo

```bash
curl -X POST "https://localhost:5001/api/Auth/validate" \
  -H "Content-Type: application/json" \
  -d '"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."'
```

#### Respuesta

```json
{ "valid": true }
```

o

```json
{ "valid": false }
```

#### Lógica interna

1. Valida la firma del JWT con `SecretKey`, `Issuer`, `Audience`, sin `ClockSkew`.
2. Busca el token en la colección `auth_tokens`.
3. Devuelve `true` solo si:
   - Existe.
   - No está revocado.
   - No está expirado.

---

## Endpoints de administración (tokens)

> Nota: a día de hoy, el servicio tiene un chequeo interno simplificado (`JUST ADMIN`) en estos métodos, por lo que hasta completar la lógica de roles puede devolver `Unauthorized`.

### 5. Revocar todos los tokens de un usuario

- **Método**: `POST`
- **Ruta**: `api/Auth/revoke/{userId}`
- **Auth**: Requiere `Authorization` y rol/admin (por implementar).

#### Request ejemplo

```bash
curl -X POST "https://localhost:5001/api/Auth/revoke/USER_ID" \
  -H "Authorization: Bearer eyJhbGciOi..." \
  -H "Content-Type: application/json"
```

- Revocará todos los tokens asociados a `USER_ID` cuando la lógica de admin esté activa.

---

### 6. Limpiar tokens expirados

- **Método**: `POST`
- **Ruta**: `api/Auth/cleanup`
- **Auth**: Requiere `Authorization` y permisos de admin (por implementar).

#### Request ejemplo

```bash
curl -X POST "https://localhost:5001/api/Auth/cleanup" \
  -H "Authorization: Bearer eyJhbGciOi..." \
  -H "Content-Type: application/json"
```

- Marca como revocados todos los tokens expirados que aún no lo estén.

---

## Resumen rápido para consumir el módulo Auth

1. **Login**  
   - `POST api/Auth/login` con `email` y `password`.  
   - Guarda `access_token.token` y `refresh_token.token`.

2. **Consumir APIs protegidas**  
   - Añade `Authorization: Bearer {access_token.token}` a tus requests a otros controladores.

3. **Refrescar el token**  
   - Cuando el access token caduque, usa `POST api/Auth/refresh` con `refresh_token`.

4. **Logout**  
   - Para cerrar todas las sesiones: `POST api/Auth/logout` con `logout_all_devices = true`.  
   - Para cerrar solo una: envía el `refresh_token` concreto.

5. **Validar tokens en servicios externos**  
   - Usa `POST api/Auth/validate` con el token en el body para saber si sigue siendo válido.


