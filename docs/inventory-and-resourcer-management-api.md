# Inventory & Resource Management — Contrato HTTP y Arquitectura

Base: `/api/v1`. Todos los cuerpos son JSON (`application/json`) y los errores usan
**RFC 7807 Problem Details** (`application/problem+json`). Las rutas siguen la convención
kebab-case del proyecto.

Este Bounded Context reemplaza los mocks del Fake API (`chemicals` y `nozzles` del `db.json`)
y cubre las Historias de Usuario:

- **DR-003**: Registro de agroquímicos e insumos (stock, stock mínimo, vencimiento y reposición).
- **DR-004**: Registro de tipos de boquillas de asperjar (tipo, diámetro, unidades y condición).

Borrado: **baja lógica** (`is_active = 0`). Un recurso dado de baja no aparece en los listados y
responde `404` al consultarlo, pero se conserva en la base de datos para historial y reportes.

---

## Agroquímicos — `/api/v1/chemicals`

| Método | Ruta | Descripción | Éxito | HU |
|---|---|---|---|---|
| `GET` | `/chemicals` | Lista los agroquímicos activos (filtro opcional `type`) | `200` | DR-003 |
| `GET` | `/chemicals/{id}` | Obtiene un agroquímico activo | `200` | DR-003 |
| `POST` | `/chemicals` | Registra un agroquímico | `201` + `Location` | DR-003 |
| `PUT` | `/chemicals/{id}` | Actualiza nombre, tipo, stock, stock mínimo o vencimiento | `200` | DR-003 |
| `DELETE` | `/chemicals/{id}` | Baja lógica del agroquímico | `204` | DR-003 |
| `POST` | `/chemicals/{id}/restock` | Repone stock sumando litros (`liters > 0`) | `200` | DR-003 |

### `ChemicalResource` (JSON)
```json
{
  "id": 1,
  "name": "Glifosato 48%",
  "type": "herbicide",
  "stockLiters": 120.0,
  "minimumStockLiters": 40.0,
  "expirationDate": "2027-08-30",
  "isActive": true,
  "createdAt": "2026-11-25T20:00:00Z",
  "updatedAt": "2026-11-25T20:00:00Z"
}
```

- `type`: `herbicide` | `insecticide` | `fungicide` | `fertilizer`.
- `expirationDate`: día calendario en formato `yyyy-MM-dd`.

### Body de `POST /chemicals/{id}/restock`
```json
{ "liters": 20 }
```

### Reglas de negocio (agregado `Chemical`)

| Regla | Error | HTTP |
|---|---|---|
| El nombre es obligatorio (máx. 100 caracteres) | `ChemicalNameRequired` / `ChemicalNameTooLong` | 400 |
| El tipo debe ser uno de los 4 permitidos | `ChemicalTypeInvalid` | 400 |
| Stock y stock mínimo `>= 0` | `ChemicalStockInvalid` / `ChemicalMinimumStockInvalid` | 400 |
| La fecha de vencimiento es obligatoria (`yyyy-MM-dd`) | `ChemicalExpirationDateRequired` | 400 |
| La reposición debe ser mayor a 0 litros | `ChemicalRestockAmountInvalid` | 400 |
| El agroquímico debe existir y estar activo | `ChemicalNotFound` | 404 |

Evento de dominio: `ChemicalRestockedEvent` (litros agregados y stock resultante).

---

## Boquillas — `/api/v1/nozzles`

| Método | Ruta | Descripción | Éxito | HU |
|---|---|---|---|---|
| `GET` | `/nozzles` | Lista las boquillas activas (filtro opcional `status`) | `200` | DR-004 |
| `GET` | `/nozzles/{id}` | Obtiene una boquilla activa | `200` | DR-004 |
| `POST` | `/nozzles` | Registra boquillas (`status` opcional, `good` por defecto) | `201` + `Location` | DR-004 |
| `PUT` | `/nozzles/{id}` | Actualiza tipo, diámetro, unidades o condición | `200` | DR-004 |
| `DELETE` | `/nozzles/{id}` | Baja lógica de la boquilla | `204` | DR-004 |
| `POST` | `/nozzles/{id}/toggle-status` | Alterna la condición entre `good` y `worn` | `200` | DR-004 |

### `NozzleResource` (JSON)
```json
{
  "id": 1,
  "type": "flat-fan",
  "diameterMm": 5.0,
  "stockUnits": 12,
  "status": "good",
  "isActive": true,
  "createdAt": "2026-11-25T20:00:00Z",
  "updatedAt": "2026-11-25T20:00:00Z"
}
```

- `type`: `flat-fan` | `hollow-cone`.
- `status`: `good` | `worn`.

### Reglas de negocio (agregado `Nozzle`)

| Regla | Error | HTTP |
|---|---|---|
| El tipo debe ser `flat-fan` o `hollow-cone` | `NozzleTypeInvalid` | 400 |
| El diámetro debe ser mayor a 0 mm | `NozzleDiameterInvalid` | 400 |
| Las unidades deben ser `>= 0` | `NozzleStockInvalid` | 400 |
| La condición debe ser `good` o `worn` | `NozzleStatusInvalid` | 400 |
| La boquilla debe existir y estar activa | `NozzleNotFound` | 404 |

Evento de dominio: `NozzleStatusChangedEvent` (condición anterior y nueva).

En el frontend, el formulario del dron (`dron-form-view.vue`) lista estas boquillas en un selector
y no permite instalar una boquilla desgastada o sin unidades.

---

## Arquitectura

```text
InventoryAndResourceManagement/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/      Chemical, Nozzle
│   │   ├── Commands/        Create/Update/Delete/Restock/ToggleStatus
│   │   ├── Errors/          InventoryErrors (catálogo de errores del BC)
│   │   ├── Events/          ChemicalRestockedEvent, NozzleStatusChangedEvent
│   │   ├── Queries/         GetById / GetAll
│   │   └── ValueObjects/    ChemicalId, ChemicalType, NozzleId, NozzleType, NozzleStatus
│   └── Repositories/        IChemicalRepository, INozzleRepository
├── Application/
│   ├── CommandServices/     interfaces
│   ├── QueryServices/       interfaces
│   └── Internal/            implementaciones (orquestan agregado + UnitOfWork)
├── Infrastructure/
│   └── Persistence/EntityFrameworkCore/
│       ├── Configuration/   mapeo a las tablas `chemicals` y `nozzles`
│       ├── Repositories/    filtran `is_active`
│       └── InventorySeeder  datos demo
└── Interfaces/Rest/
    ├── Controllers/         ChemicalsController, NozzlesController
    ├── Resources/           JSON de entrada y salida
    └── Transform/           assemblers (enum ↔ texto, fecha ↔ yyyy-MM-dd)
```

Las reglas de negocio viven en los agregados; los controladores solo traducen JSON y los
servicios de aplicación orquestan la transacción.

## Persistencia

| Tabla | Migración | Columnas |
|---|---|---|
| `chemicals` | `AddInventoryChemicals` | `id`, `name`, `type`, `stock_liters`, `minimum_stock_liters`, `expiration_date` (DATE), `is_active`, `created_at`, `updated_at` |
| `nozzles` | `AddInventoryNozzles` | `id`, `type`, `diameter_mm`, `stock_units`, `status`, `is_active`, `created_at`, `updated_at` |

Los enums se guardan como texto (`Herbicide`, `FlatFan`, `Good`…) y el assembler los expone en
minúscula/kebab-case como los usa el frontend.
