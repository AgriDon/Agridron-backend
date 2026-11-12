# Field Management — catálogo de eventos

Los eventos de dominio se generan dentro de los agregados y se publican **después** de confirmar
la transacción (`IUnitOfWork.CompleteAsync`). Los eventos de integración son el contrato hacia
otros bounded contexts.

## Eventos de dominio

| Evento | Se emite cuando | Agregado |
|---|---|---|
| `FarmCreatedEvent` | Se registra una finca | `Farm` |
| `FarmRenamedEvent` | Cambia el nombre de la finca | `Farm` |
| `FarmOwnerChangedEvent` | Cambia el dueño de la finca | `Farm` |
| `CropCreatedEvent` | Se registra un cultivo | `Crop` |
| `ParcelCreatedEvent` | Se registra una parcela | `Parcel` |
| `ParcelGeometryUpdatedEvent` | Se redefine el polígono y cambia el área | `Parcel` |
| `ParcelCropAssignedEvent` | Se asigna o cambia el cultivo | `Parcel` |
| `FumigationAreaDefinedEvent` | Se define un área de fumigación | `FumigationArea` |
| `FumigationAreaRedefinedEvent` | Se modifica el polígono del área | `FumigationArea` |

## Eventos de integración

Se publican hacia otros BCs. El payload viaja en JSON y **no** incluye entidades de dominio.

### `FieldManagement.FarmCreated`

```json
{
  "farmId": 1,
  "ownerId": 1,
  "name": "Finca San Rafael",
  "location": "Ica, Peru",
  "occurredAt": "2026-11-03T10:00:00Z"
}
```

Consumidores previstos: Analytics & Reporting (dimensión finca), IAM (validación de dueño).

### `FieldManagement.ParcelGeometryUpdated`

```json
{
  "parcelId": 1,
  "farmId": 1,
  "cropId": 1,
  "areaHectares": 12.5,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[...]}",
  "occurredAt": "2026-11-03T10:00:00Z"
}
```

Consumidores previstos: Flight Operations (validar que la misión cae dentro de la parcela),
Analytics & Reporting (hectáreas tratadas).

### `FieldManagement.FumigationAreaDefined`

```json
{
  "fumigationAreaId": 1,
  "parcelId": 1,
  "areaHectares": 3.2,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[...]}",
  "occurredAt": "2026-11-03T10:00:00Z"
}
```

Consumidores previstos: Flight Operations (planificación de misión), Inventory (cálculo de dosis).

## Reglas

- El nombre de un evento de integración usa el prefijo del BC: `FieldManagement.<Evento>`.
- Los eventos son inmutables.
- Un evento publicado no se modifica: si cambia el contrato, se versiona.
- Los handlers son idempotentes.
- El fallo de un consumidor **no** revierte la transacción del productor.
