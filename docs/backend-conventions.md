# Convenciones de backend

Este documento es la **fuente de verdad** para cualquier persona o agente de IA que escriba
código en este repositorio. Si algo no está aquí, se discute en un ADR antes de implementarlo.

> Regla de oro: **un cambio, un commit; un commit, una idea**. Todo commit debe compilar y dejar
> la suite de pruebas en verde.

---

## 1. Stack

| Elemento | Decisión |
|---|---|
| Runtime | .NET 10 |
| Lenguaje | C# 14 |
| Web | ASP.NET Core controllers |
| ORM | Entity Framework Core 10 |
| Base de datos | MySQL 8 |
| Mediador | Cortex.Mediator |
| Documentación | Swashbuckle (OpenAPI 3) |
| Pruebas | xUnit + FluentAssertions + Testcontainers |

No se agregan dependencias nuevas sin justificarlo en un ADR y acordarlo con el equipo.

---

## 2. Estructura del repositorio

```
Agridron.Backend.API/
├── <BoundedContext>/
│   ├── Domain/
│   │   ├── Model/
│   │   │   ├── Aggregates/
│   │   │   ├── Entities/
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Exceptions/
│   │   └── Repositories/
│   ├── Application/
│   │   ├── CommandServices/
│   │   ├── QueryServices/
│   │   └── Internal/
│   │       ├── CommandServices/
│   │       ├── QueryServices/
│   │       └── EventHandlers/
│   ├── Infrastructure/
│   │   └── Persistence/EntityFrameworkCore/
│   │       ├── Configuration/
│   │       ├── Repositories/
│   │       └── Seed/
│   └── Interfaces/
│       └── Rest/
│           ├── Controllers/
│           ├── Resources/
│           └── Transform/
└── Shared/
```

Reglas:

- Un tipo por archivo. El nombre del archivo coincide con el tipo.
- El espacio de nombres sigue la ruta física:
  `Agridron.Backend.API.<BoundedContext>.<Capa>.<Subcarpeta>`.
- `Shared` **no** puede referenciar ningún bounded context.
- Un bounded context **no** puede referenciar las clases de dominio de otro. Solo se comunican
  por IDs, eventos de integración o fachadas (ACL).

---

## 3. Regla de dependencia

```
Interfaces ──▶ Application ──▶ Domain
                    │
                    ▼
             Infrastructure (implementa puertos del dominio)
```

- `Domain` no conoce EF Core, ASP.NET, JSON ni HTTP.
- `Application` orquesta casos de uso y depende solo de abstracciones del dominio.
- `Infrastructure` implementa repositorios y clientes externos.
- `Interfaces` traduce HTTP ↔ aplicación. Nunca contiene reglas de negocio.

---

## 4. Nomenclatura

| Elemento | Convención | Ejemplo |
|---|---|---|
| Clases, records, enums | PascalCase | `FumigationArea` |
| Interfaces | `I` + PascalCase | `IFarmRepository` |
| Métodos asíncronos | sufijo `Async` | `GetByIdAsync` |
| Campos privados | `_camelCase` | `_unitOfWork` |
| Parámetros y locales | camelCase | `farmId` |
| Constantes | PascalCase | `MaxNameLength` |
| JSON expuesto | camelCase | `ownerId` |
| Tablas | snake_case plural | `fumigation_areas` |
| Columnas | snake_case singular | `created_at` |
| Rutas HTTP | kebab-case plural bajo `/api/v1` | `/api/v1/fumigation-areas` |
| Migraciones | PascalCase descriptivo | `InitialFieldManagement` |

---

## 5. Dominio

### Agregados

- Heredan de `AggregateRoot<TId>`.
- Tienen un constructor privado sin parámetros para EF Core y un constructor público/factoría
  que valida invariantes.
- Todas las propiedades de colección son `private set` y se exponen como `IReadOnlyCollection<T>`.
- El estado solo cambia mediante métodos con intención de negocio (`Rename`, `RedefineGeometry`),
  nunca con setters públicos.
- No se usan atributos de EF Core en el dominio. El mapeo vive en `Infrastructure`.

### Value objects

- Heredan de `ValueObject`.
- Son inmutables (`record` o clase con `init`).
- Se validan en el constructor y lanzan `DomainException` si son inválidos.
- Se comparan por valor.

### Identificadores

- Cada agregado usa un ID tipado (`FarmId`, `ParcelId`, …), nunca `long` suelto dentro del dominio.
- En la base de datos se persisten como `BIGINT`.

### Eventos de dominio

- Se nombran en pasado: `FarmCreatedEvent`, `ParcelGeometryUpdatedEvent`.
- Se agregan con `AddDomainEvent(...)` dentro del agregado.
- Se publican **después** de confirmar la transacción, nunca antes.

### Reglas

- El dominio no lanza excepciones para flujos esperados. Devuelve `Result<T>` con un error del
  catálogo del bounded context.
- Las excepciones se reservan para errores de programación o infraestructura.

---

## 6. Aplicación

- Un servicio de comandos y uno de consultas por agregado.
- Los comandos se nombran `CreateFarmCommand`, `UpdateParcelCommand`, `DeleteCropCommand`.
- Las consultas se nombran `GetFarmByIdQuery`, `GetAllParcelsQuery`.
- Los handlers devuelven `Result<T>` o `Result`.
- Los errores esperados se modelan con el record `Error` (`Code`, `Message`, `Type`, `Details`).
  El `Code` es único dentro del bounded context; el `Type` (`Validation`, `NotFound`, `Conflict`,
  `BusinessRule`, `Unexpected`) determina el código HTTP. Nunca se usan strings mágicos ni
  excepciones para flujos esperados.
- La transacción se confirma con `IUnitOfWork.CompleteAsync()` una sola vez por caso de uso.
- Las consultas **no** pasan por el agregado: leen proyecciones/DTOs directamente.

---

## 7. Infraestructura y persistencia

- Cada agregado tiene su `IEntityTypeConfiguration<T>` en
  `Infrastructure/Persistence/EntityFrameworkCore/Configuration`.
- Las configuraciones se aplican automáticamente con
  `builder.ApplyConfigurationsFromAssembly(...)`. No se edita `AppDbContext` para agregar un BC.
- Nombres de tabla y columna: snake_case plural/singular (convención global).
- Enums persistidos como `VARCHAR` con `HasConversion<string>()`.
- Fechas en UTC (`DateTime.UtcNow`), columnas `DATETIME(6)`.
- Dinero y áreas con `DECIMAL`, nunca `double`.
- **No se crean claves foráneas entre bounded contexts.** Solo dentro del mismo BC.
  Las referencias externas son IDs con índice, sin FK.
- Toda entidad auditable implementa `IAuditableEntity`; el interceptor llena `created_at` y
  `updated_at`.
- Borrado: `DeleteBehavior.Restrict` por defecto. Nada de cascadas accidentales.

### Migraciones

```bash
dotnet ef migrations add InitialFieldManagement \
  --project Agridron.Backend.API \
  --output-dir FieldManagement/Infrastructure/Persistence/EntityFrameworkCore/Migrations

dotnet ef database update --project Agridron.Backend.API
```

- Una migración por bounded context y por cambio de esquema.
- El nombre describe el cambio: `InitialFieldManagement`, `AddParcelSpatialIndex`.
- Las migraciones se revisan antes de commitear: nunca se commitea una migración sin leer el SQL.
- No se edita una migración ya aplicada en `develop`. Se crea otra.

---

## 8. API REST

- Prefijo versionado obligatorio: `/api/v1`.
- Rutas en plural y kebab-case: `/api/v1/farms`, `/api/v1/fumigation-areas`.
- Un controlador por recurso. El controlador solo traduce HTTP ↔ aplicación.
- Nunca se expone una entidad de dominio: siempre `Resource` + `Assembler`.
- Resources de entrada y salida separados:
  `CreateFarmResource`, `UpdateFarmResource`, `FarmResource`.

### Códigos de estado

| Caso | Código |
|---|---|
| Consulta correcta | `200 OK` |
| Creación correcta | `201 Created` + `Location` |
| Eliminación correcta | `204 No Content` |
| Cuerpo inválido | `400 Bad Request` |
| No autenticado | `401 Unauthorized` |
| Sin permisos | `403 Forbidden` |
| Recurso inexistente | `404 Not Found` |
| Conflicto de estado o duplicado | `409 Conflict` |
| Regla de negocio violada | `422 Unprocessable Entity` |
| Error inesperado | `500 Internal Server Error` |

### Errores

Todos los errores usan **RFC 7807 Problem Details** (`application/problem+json`):

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Farm with id 42 was not found.",
  "instance": "/api/v1/farms/42",
  "code": "FarmNotFound"
}
```

La extensión `code` viene del catálogo de errores del bounded context y el status se deriva
del `ErrorType`:

| `ErrorType` | HTTP |
|---|---|
| `Validation` | `400 Bad Request` |
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| `BusinessRule` | `422 Unprocessable Entity` |
| `Unexpected` | `500 Internal Server Error` |

### Paginación

- Sin parámetros: se devuelve la colección completa (compatibilidad con el frontend actual).
- Con `?page=0&size=20`: se devuelve la página y la cabecera `X-Total-Count`.
- `size` máximo: 100.

### Documentación

- Todo endpoint lleva `[ProducesResponseType]` para cada código posible.
- Todo recurso público lleva `[SwaggerSchema]` y un ejemplo cuando el formato no es obvio.
- El JSON de Swagger es parte del contrato: se valida en pruebas.

---

## 9. Validación

- Formato y obligatoriedad: DataAnnotations en los `*Resource`.
- Invariantes de negocio: en el dominio.
- Existencia de referencias: en la capa de aplicación.
- Un `400` nunca debe llegar al dominio: se corta en el modelo de la API.

---

## 10. Internacionalización

- Los mensajes viven en `Shared/Resources` (`.resx`) con variantes `en` y `es`.
- El idioma se resuelve por la cabecera `Accept-Language`.
- Nunca se escriben mensajes de error en español o inglés directamente en el código.

---

## 11. Logging

- `ILogger<T>` inyectado, nunca `Console.WriteLine`.
- Se registran eventos de negocio relevantes con `Information`.
- Los errores esperados (`Result` fallido) no se registran como `Error`.
- Nunca se registran contraseñas, tokens ni datos personales completos.

---

## 12. Pruebas

| Tipo | Herramienta | Qué cubre |
|---|---|---|
| Unitarias de dominio | xUnit + FluentAssertions | invariantes, value objects, máquinas de estado |
| Unitarias de aplicación | xUnit + mocks | orquestación, errores esperados |
| Integración de persistencia | Testcontainers MySQL | mapeos, migraciones, repositorios |
| Integración de API | `WebApplicationFactory` | rutas, códigos, contrato JSON |

Convenciones:

- Nombre: `Metodo_DeberiaResultado_CuandoCondicion`.
- Estructura AAA (`Arrange`, `Act`, `Assert`) con comentarios de sección.
- Un archivo de pruebas por clase bajo prueba.
- Sin `Thread.Sleep` ni dependencias de red reales.

---

## 13. Git

### Ramas

- `main`: versiones liberadas.
- `develop`: integración.
- `feature/<bc>-<descripcion>`: trabajo por bounded context.
- `hotfix/<descripcion>`: correcciones urgentes.

### Commits

[Conventional Commits](https://www.conventionalcommits.org/) con scope obligatorio:

```
<tipo>(<scope>): <descripción en imperativo>
```

| Tipo | Uso |
|---|---|
| `feat` | funcionalidad nueva |
| `fix` | corrección de un defecto |
| `refactor` | cambio interno sin alterar comportamiento |
| `test` | pruebas nuevas o modificadas |
| `docs` | documentación |
| `chore` | tooling, dependencias, configuración |
| `ci` | pipelines |
| `perf` | rendimiento |

Scopes permitidos: `shared`, `field-management`, `flight-operations`, `iam`, `weather`,
`analytics`, `inventory`, `repo`, `ci`.

Ejemplos:

```
feat(field-management): add parcel aggregate with geometry and area
test(field-management): cover polygon validation rules
chore(repo): add docker compose for local mysql 8
docs: add backend conventions guide
```

Reglas:

- Un commit = un cambio lógico.
- El mensaje describe **qué** y **por qué**, no cómo.
- No se commitea código comentado, `TODO` sin dueño ni archivos generados.
- No se commitean secretos. `appsettings.*.json` solo contiene valores de desarrollo.

---

## 14. Definition of Done

Un cambio está terminado cuando:

- [ ] Compila sin advertencias nuevas.
- [ ] Tiene pruebas que cubren el comportamiento agregado.
- [ ] `dotnet test` pasa en local.
- [ ] Los endpoints están documentados en Swagger.
- [ ] Los errores usan Problem Details.
- [ ] No se rompe el contrato que consume el frontend.
- [ ] La documentación del BC está actualizada.
- [ ] El commit sigue Conventional Commits.

---

## 15. Checklist para agentes de IA

Antes de generar código:

1. Lee este documento y el ADR correspondiente.
2. Identifica el bounded context y la capa donde va el cambio.
3. Revisa si ya existe un agregado, VO o error equivalente: reutilízalo.
4. No inventes endpoints fuera del contrato documentado en `docs/`.
5. No agregues dependencias sin ADR.
6. No modifiques el dominio de otro bounded context.
7. Escribe la prueba junto con el código, no después.
8. Verifica `dotnet build` y `dotnet test` antes de proponer el commit.
9. Redacta el commit con el formato de la sección 13.
