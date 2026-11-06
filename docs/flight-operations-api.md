# Flight Operations — Contrato HTTP y Arquitectura

Base: `/api/v1`. Todos los cuerpos son JSON (`application/json`) y los errores usan
**RFC 7807 Problem Details** (`application/problem+json`). Las rutas siguen la convención
kebab-case del proyecto.

Este Bounded Context reemplaza por completo los mocks del Fake API del Sprint 2 y cubre todas las
Historias de Usuario del **Sprint 3 (34 Story Points)** y Sprint 2 para operaciones de vuelo:
- **MI-002** (5 pts): Asignación de recursos (dron, insumos, dosis) a la orden de trabajo.
- **MI-003** (3 pts): Asignación de operador/piloto técnico responsable.
- **MI-004** (5 pts): Configuración de parámetros de vuelo y aspersión (altitud, velocidad, tasa, solapamiento).
- **MI-007** (3 pts): Aprobación previa de misión antes del despegue.
- **TR-001** (5 pts): Visualización y seguimiento en tiempo real del estado de vuelo.
- **TR-002** (5 pts): Monitoreo de telemetría (batería, altitud, velocidad, GPS).
- **TR-003** (3 pts): Generación automática de alertas por desvío de parámetros o batería crítica.
- **TR-005** (5 pts): Registro del historial y finalización de misión con cálculo de superficie tratada.
- **TR-004**: Pausa y reanudación segura de misión guardando coordenadas.
- **MI-006**: Cancelación de misión y liberación de dron.
- **DR-001 / DR-002**: Catálogo y gestión de flota de drones.
- **MI-001 / MI-005**: Planificación y listado de misiones de vuelo.

---

## Drones — `/api/v1/drones`

| Método | Ruta | Descripción | Éxito | HUs |
|---|---|---|---|---|
| `GET` | `/drones` | Lista drones de la flota (filtros: `status`, `minCapacity`, `maxCapacity`) | `200` | DR-001 |
| `GET` | `/drones/{id}` | Obtiene detalle de un dron específico | `200` | DR-001 |
| `POST` | `/drones` | Registra un nuevo dron en la flota | `201` + `Location` | DR-002 |
| `PUT` | `/drones/{id}` | Actualiza especificaciones técnicas, mantenimiento o foto | `200` | DR-002 |
| `DELETE` | `/drones/{id}` | Inactiva un dron (soft delete) si no está en vuelo o asignado | `204` | DR-002 |

### `DroneResource` (JSON)
```json
{
  "id": 1,
  "serialNumber": "SN-DJI-T40-001",
  "model": "DJI Agras T40",
  "capacity": 40.0,
  "capacityLiters": 40.0,
  "status": "Available",
  "battery": 95.0,
  "batteryLevel": 95.0,
  "flightHours": 14.5,
  "nozzleId": 2,
  "lastMaintenanceDate": "2026-10-15T08:00:00Z",
  "nextMaintenanceDate": "2026-12-15T08:00:00Z",
  "imageUrl": "https://cdn.agridron.com/drones/t40.jpg",
  "urlimg": "https://cdn.agridron.com/drones/t40.jpg",
  "isActive": true,
  "createdAt": "2026-11-06T15:00:00Z",
  "updatedAt": "2026-11-06T15:00:00Z"
}
```

---

## Misiones de Vuelo — `/api/v1/missions`

| Método | Ruta | Descripción | Éxito | HUs |
|---|---|---|---|---|
| `GET` | `/missions` | Lista misiones (filtros: `farmId`, `parcelId`, `droneId`, `status`, `from`, `to`) | `200` | MI-001, MI-005 |
| `GET` | `/missions/{id}` | Obtiene una misión por su ID con parámetros y estado | `200` | MI-001 |
| `POST` | `/missions` | Planifica una nueva misión de vuelo (enriquece snapshot de parcela por ACL) | `201` + `Location` | MI-001 |
| `PUT` | `/missions/{id}` | Actualiza datos generales o notas de una misión | `200` | MI-001 |
| `DELETE` | `/missions/{id}` | Elimina o cancela una misión no iniciada | `204` | MI-006 |
| `POST` | `/missions/{id}/resources` | Asigna dron, producto químico y dosis a la misión | `200` | **MI-002** |
| `POST` | `/missions/{id}/operator` | Asigna piloto/operador técnico responsable | `200` | **MI-003** |
| `POST` | `/missions/{id}/flight-parameters` | Configura altitud (1-30m), velocidad (1-20m/s), tasa (1-100L/ha), solape (0-80%) | `200` | **MI-004** |
| `POST` | `/missions/{id}/approve` | Autoriza la misión para el despegue | `200` | **MI-007** |
| `POST` | `/missions/{id}/start` | Inicia la misión en campo (dron pasa a `InFlight`) | `200` | **TR-001** |
| `POST` | `/missions/{id}/pause` | Pausa la misión y guarda punto de coordenadas (dron pasa a `Paused`) | `200` | **TR-004** |
| `POST` | `/missions/{id}/resume` | Reanuda la misión en ejecución (dron regresa a `InFlight`) | `200` | **TR-004** |
| `POST` | `/missions/{id}/complete` | Finaliza la misión, registra hectáreas tratadas y libera el dron a `Available` | `200` | **TR-005** |
| `POST` | `/missions/{id}/cancel` | Cancela la misión por clima/emergencia y libera el dron | `200` | **MI-006** |

### `MissionResource` (JSON)
```json
{
  "id": 10,
  "code": "MS-20261106-1042",
  "farmId": 1,
  "parcelId": 5,
  "droneId": 1,
  "chemicalId": 4,
  "operatorId": 12,
  "farmArea": "Lote San Rafael 01",
  "cropType": "Maíz Amarillo",
  "operatorName": "Carlos Mendoza",
  "treatmentType": "Fumigación Foliar",
  "productName": "Fungicida Sistémico Pro",
  "productDose": 2.5,
  "notes": "Aplicación preventiva al amanecer.",
  "status": "Authorized",
  "scheduledAt": "2026-11-07T06:00:00Z",
  "plannedAreaHectares": 12.5,
  "treatedAreaHectares": 0.0,
  "progress": 0,
  "flightParameters": {
    "altitudeMeters": 3.5,
    "speedMetersPerSecond": 5.0,
    "sprayRateLitersPerHectare": 15.0,
    "overlapPercentage": 20.0
  },
  "lastPausePoint": null,
  "startedAt": null,
  "completedAt": null,
  "isActive": true,
  "createdAt": "2026-11-06T15:30:00Z",
  "updatedAt": "2026-11-06T15:35:00Z"
}
```

---

## Telemetría y Operaciones en Tiempo Real — `/api/v1/operations`

| Método | Ruta | Descripción | Éxito | HUs |
|---|---|---|---|---|
| `GET` | `/operations` | Lista lecturas de telemetría (filtros: `missionId`, `droneId`) | `200` | **TR-001, TR-002** |
| `GET` | `/operations/{id}` | Obtiene punto específico de telemetría | `200` | **TR-002** |
| `POST` | `/operations` | Registra posición GPS y telemetría (dispara alerta automática si batería < 15% o desvío de altitud) | `201` + `Location` | **TR-001, TR-002, TR-003** |
| `PUT` | `/operations/{id}` | Actualiza coordenadas de la operación | `200` | **TR-001** |
| `DELETE` | `/operations/{id}` | Elimina registro de telemetría | `204` | **TR-001** |

### `OperationStatusResource` (JSON)
```json
{
  "id": 105,
  "missionId": 10,
  "droneId": 1,
  "latitude": -12.046374,
  "longitude": -77.042793,
  "altitudeMeters": 3.6,
  "speedMetersPerSecond": 4.8,
  "batteryLevel": 82.0,
  "tankVolumeLiters": 34.5,
  "status": "EN_CURSO",
  "timestamp": "2026-11-07T06:15:30Z",
  "createdAt": "2026-11-07T06:15:30Z",
  "updatedAt": "2026-11-07T06:15:30Z"
}
```

---

## Incidentes y Alertas de Vuelo — `/api/v1/incidents`

| Método | Ruta | Descripción | Éxito | HUs |
|---|---|---|---|---|
| `GET` | `/incidents` | Lista incidentes y anomalías de vuelo (filtros: `missionId`, `droneId`, `type`, `severity`) | `200` | **TR-003** |
| `GET` | `/incidents/{id}` | Obtiene detalle de un incidente | `200` | **TR-003** |
| `POST` | `/incidents` | Reporta anomalía manual o incidente de campo | `201` + `Location` | **TR-003** |
| `PUT` | `/incidents/{id}` | Actualiza descripción o resolución del incidente | `200` | **TR-003** |
| `DELETE` | `/incidents/{id}` | Elimina registro de incidente | `204` | **TR-003** |

### `IncidentResource` (JSON)
```json
{
  "id": 14,
  "type": "Batería Crítica",
  "description": "Nivel de batería crítico reportado a 8.5% en telemetría.",
  "occurredAt": "2026-11-07T06:35:10Z",
  "missionId": 10,
  "droneId": 1,
  "severity": "Critical",
  "createdAt": "2026-11-07T06:35:10Z",
  "updatedAt": "2026-11-07T06:35:10Z"
}
```

---

## Anti-Corruption Layer (ACL) entre Bounded Contexts

`FlightOperations` interactúa con `FieldManagement` mediante el adaptador `IExternalFieldService` y la fachada `IFieldManagementContextFacade`:
- `missions` guarda `farm_id` y `parcel_id` como índices lógicos sin clave foránea física en MySQL.
- En la creación y edición de misiones, el ACL valida la existencia de la finca y la parcela, garantizando integridad referencial y enriqueciendo el nombre de la parcela y cultivo sin acoplar los modelos de dominio.
