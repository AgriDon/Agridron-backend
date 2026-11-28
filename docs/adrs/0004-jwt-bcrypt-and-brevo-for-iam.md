# 0004 — JWT Bearer, BCrypt y Brevo (HTTP nativo) para el Bounded Context IAM

- **Estado:** Aceptado
- **Fecha:** 2026-11-27
- **Decisores:** equipo de backend

## Contexto

El Bounded Context **IAM** (*Identity & Access Management*) requiere autenticación *stateless*
para proteger los endpoints de la API REST consumidos por el frontend SPA, almacenamiento seguro de
credenciales en MySQL y envío de correos transaccionales (verificación de cuenta, recuperación de
contraseña y notificaciones de seguridad).

De acuerdo con `docs/backend-conventions.md`, ninguna dependencia externa se incorpora sin
justificación explícita en un ADR, y el monolito modular debe mantenerse ligero evitando SDKs
innecesarios o secretos en texto plano dentro del repositorio.

## Opciones consideradas

| Opción | Ventajas | Desventajas |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Paquete oficial de Microsoft alineado al runtime .NET 10; integración nativa con `[Authorize]`, políticas y middleware de ASP.NET Core; autenticación *stateless* | Requiere rotación/gestión segura de `JWT_SECRET` vía variables de entorno o *user-secrets* |
| Sesiones con cookies en servidor | Revocación inmediata en servidor | Acopla el backend a estado de sesión y complica clientes SPA/móviles |
| `BCrypt.Net-Next` | Hashing adaptativo con *salt* único por contraseña y factor de coste configurable; amplio soporte en .NET sin dependencias nativas frágiles | Ligeramente más lento que SHA-256 (deseable por diseño frente a fuerza bruta) |
| ASP.NET Core Identity completo | Incluye tablas y flujos preconstruidos | Impone esquema de tablas y dependencias de infraestructura que rompen el modelo DDD puro del BC `IAM` |
| `IHttpClientFactory` nativo contra API REST v3 de Brevo | Cero dependencias NuGet adicionales; control total sobre timeouts, headers (`api-key`), payloads JSON y pruebas con handlers simulados | Mapeo manual de los DTOs de envío de correo (contrato pequeño y estable) |
| SDK oficial/terceros de Brevo | Clases autogeneradas para toda la API | Arrastra dependencias transitivas innecesarias para un único endpoint (`POST /v3/smtp/email`) |

## Decisión

1. **Autenticación JWT:** Adoptar `Microsoft.AspNetCore.Authentication.JwtBearer` (versión `10.0.x`)
   para autenticar solicitudes mediante tokens Bearer firmados con HMAC-SHA256. La clave secreta se
   inyecta exclusivamente por variables de entorno (`JWT_SECRET` o `Jwt__Secret`) o `dotnet user-secrets`
   en desarrollo; nunca se versiona en `appsettings*.json`.
2. **Hashing de contraseñas:** Adoptar `BCrypt.Net-Next` (`4.0.3`) para generar y verificar hashes de
   contraseñas con *salt* aleatorio y coste adaptativo, encapsulado detrás de un puerto/servicio en el
   Bounded Context `IAM`.
3. **Correos transaccionales (Brevo):** Consumir directamente la API REST v3 de Brevo
   (`https://api.brevo.com/v3/smtp/email`) mediante un cliente HTTP nombrado (`"Brevo"`) registrado
   con `IHttpClientFactory` nativo de .NET, con bandera `Brevo:Enabled` (`false` por defecto) y clave
   inyectada por `BREVO_API_KEY` (o `Brevo__ApiKey`).

## Consecuencias

- **Positivas:**
  - Mínimo número de paquetes externos (solo 2 dependencias bien mantenidas).
  - Cero secretos en el repositorio: la aplicación arranca sin credenciales reales en desarrollo y
    rechaza tokens inválidos con RFC 7807 Problem Details (`401` / `403`).
  - Integración limpia con Swagger/OpenAPI (esquema Bearer) y con el patrón `Result<T>` de `Shared`
    (extendido con `ErrorType.Unauthorized` y `ErrorType.Forbidden`).
- **Negativas / deuda asumida:**
  - Los tokens JWT emitidos son válidos hasta su expiración salvo que se incorpore posteriormente
    una lista de revocación o *refresh tokens*.
  - En producción será obligatorio definir `JWT_SECRET` (mínimo 32 caracteres) y `BREVO_API_KEY`
    cuando `Brevo:Enabled` sea `true`.
- **Seguimiento:**
  - Implementar los agregados, servicios de aplicación, adaptador HTTP de Brevo y endpoints de
    autenticación dentro de `Agridron.Backend.API/Iam/`.
