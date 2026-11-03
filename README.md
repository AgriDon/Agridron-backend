# AgriDron Backend

Backend de **AgriDron Solutions**: monolito modular construido con **C# 14**, **ASP.NET Core 10**
y **MySQL 8**. Implementa DDD táctico y CQRS ligero por *bounded context*.

Este repositorio contiene el **shared kernel** (base técnica común) sobre el que se implementan
los seis bounded contexts del producto.

## Stack

| Elemento | Versión / decisión |
|---|---|
| .NET | 10 (SDK 10.0.403) |
| Lenguaje | C# 14 |
| Framework web | ASP.NET Core 10 (controladores) |
| Persistencia | Entity Framework Core 10 |
| Proveedor de base de datos | `MySql.EntityFrameworkCore` 10 |
| Base de datos | MySQL 8 (columna espacial nativa `GEOMETRY`) |
| Migraciones | EF Core Migrations |
| Mediador | Cortex.Mediator |
| Documentación API | Swashbuckle / OpenAPI 3 |
| Pruebas | xUnit, FluentAssertions, Testcontainers |
| Arquitectura | Monolito modular + DDD por capas + CQRS ligero |

## Requisitos

- .NET SDK 10 (`dotnet --version` debe reportar `10.0.x`)
- Docker y Docker Compose
- (Opcional) `dotnet-ef` como herramienta global: `dotnet tool install --global dotnet-ef`

## Inicio rápido

```bash
cp .env.example .env

# Base de datos MySQL
docker compose up -d mysql

# Restaurar dependencias
dotnet restore Agridron.Backend.slnx

# Aplicar migraciones (disponibles a partir de la Fase 3)
dotnet ef database update --project Agridron.Backend.API

# Aplicación
dotnet run --project Agridron.Backend.API
```

La API queda disponible en `http://localhost:5000`.

| Recurso | URL |
|---|---|
| Swagger UI | `http://localhost:5000/swagger` |
| OpenAPI JSON | `http://localhost:5000/swagger/v1/swagger.json` |
| API (prefijo versionado) | `http://localhost:5000/api/v1` |

## Estructura

```text
Agridron.Backend.API
├── Program.cs                         # composición de la aplicación
├── FieldManagement/                   # fincas, parcelas, cultivos y áreas de fumigación
│   ├── Domain/                        # agregados, value objects, eventos, repositorios
│   ├── Application/                   # command/query services
│   ├── Infrastructure/Persistence/    # configuraciones EF Core, repositorios, seed
│   └── Interfaces/Rest/               # controladores, resources y assemblers
├── FlightOperations/                  # misiones, drones, incidentes, telemetría
├── IdentityAccess/                    # usuarios, roles, JWT
├── WeatherIntegration/                # clima y alertas
├── AnalyticsReporting/                # reportes, historias y métricas
├── InventoryManagement/               # químicos, boquillas y mantenimiento
└── Shared/                            # shared kernel
    ├── Application/Model              # Result<T> y utilidades de aplicación
    ├── Domain/Model                   # Entity, AggregateRoot, ValueObject, Error
    ├── Domain/Repositories            # IBaseRepository, IUnitOfWork
    ├── Infrastructure/Persistence     # AppDbContext, base repository, interceptor de auditoría
    ├── Infrastructure/Pipeline        # middleware global de excepciones
    ├── Interfaces/Rest                # Problem Details (RFC 7807)
    └── Resources                      # mensajes i18n (en/es)
```

Cada bounded context replica las capas `Domain`, `Application`, `Infrastructure` e `Interfaces`.
La regla de dependencia apunta siempre hacia el dominio: `Interfaces → Application → Domain`.

## Bounded contexts

| Bounded context | Tipo | Estado |
|---|---|---|
| Shared kernel | — | ✅ Base lista |
| Field Management | Supporting | 🚧 En desarrollo |
| Identity & Access Management | Generic | ⏳ Pendiente |
| Flight Operations | Core | ⏳ Pendiente |
| Weather Integration | Supporting | ⏳ Pendiente |
| Analytics & Reporting | Supporting | ⏳ Pendiente |
| Inventory & Resource Management | Supporting | ⏳ Pendiente |

## Pruebas

```bash
dotnet test Agridron.Backend.slnx
```

Las pruebas de integración usan MySQL en contenedor (Testcontainers) y se omiten
automáticamente si Docker no está disponible.

## Convenciones

Antes de escribir código en un bounded context, lee
[`docs/backend-conventions.md`](docs/backend-conventions.md).

Decisiones de arquitectura registradas en [`docs/adrs`](docs/adrs).

El mapeo de persistencia de Field Management está en
[`docs/field-persistence.md`](docs/field-persistence.md), su contrato HTTP en
[`docs/field-api.md`](docs/field-api.md) y su catálogo de eventos de integración en
[`docs/field-events.md`](docs/field-events.md).

## Ramas

- `main`: versiones liberadas.
- `develop`: integración.
- `feature/<bc>-<descripcion>`: trabajo por bounded context.
- `hotfix/<descripcion>`: correcciones urgentes.

Los commits siguen [Conventional Commits](https://www.conventionalcommits.org/).
