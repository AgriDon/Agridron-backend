# Field Management — contrato HTTP

Base: `/api/v1`. Todos los cuerpos son JSON (`application/json`) y los errores usan
**RFC 7807 Problem Details** (`application/problem+json`). Las rutas siguen la convención
kebab-case del proyecto.

Todos los identificadores son `long` y las áreas se devuelven en **hectáreas** con dos
decimales. La geometría viaja como **string GeoJSON** (`Polygon`) y el servidor calcula el área;
el cliente nunca envía `areaHectares`.

## Fincas — `/api/v1/farms`

| Método | Ruta | Descripción | Éxito |
|---|---|---|---|
| `GET` | `/farms` | Lista fincas; filtro opcional `ownerId` | `200` |
| `GET` | `/farms/{id}` | Obtiene una finca | `200` |
| `POST` | `/farms` | Registra una finca | `201` + `Location` |
| `PUT` | `/farms/{id}` | Reemplaza los datos de una finca | `200` |
| `DELETE` | `/farms/{id}` | Elimina una finca sin parcelas | `204` |

`FarmResource`:

```json
{
  "id": 1,
  "name": "Finca San Rafael",
  "location": "Ica, Peru",
  "ownerId": 12,
  "imageUrl": "https://cdn.example.com/farms/san-rafael.jpg"
}
```

`POST`/`PUT` reciben `name`, `location`, `ownerId` e `imageUrl` (opcional). `imageUrl` debe ser
una URL absoluta `http` o `https`. `DELETE` responde `409 Conflict` con
`FarmHasParcels` cuando la finca todavía tiene parcelas.

## Cultivos — `/api/v1/crops`

| Método | Ruta | Descripción | Éxito |
|---|---|---|---|
| `GET` | `/crops` | Lista cultivos | `200` |
| `GET` | `/crops/{id}` | Obtiene un cultivo | `200` |
| `POST` | `/crops` | Registra un cultivo | `201` + `Location` |
| `PUT` | `/crops/{id}` | Actualiza un cultivo | `200` |
| `DELETE` | `/crops/{id}` | Elimina un cultivo sin parcelas | `204` |

`CropResource`:

```json
{
  "id": 3,
  "name": "Maiz",
  "variety": "Amarillo"
}
```

La combinación `name` + `variety` es única. `POST`/`PUT` responden `409 Conflict` con
`CropAlreadyExists` si ya existe. `DELETE` responde `409 Conflict` con `CropHasParcels` si el
cultivo está asignado a alguna parcela.

## Parcelas — `/api/v1/parcels`

| Método | Ruta | Descripción | Éxito |
|---|---|---|---|
| `GET` | `/parcels` | Lista parcelas; filtro opcional `farmId` | `200` |
| `GET` | `/parcels/{id}` | Obtiene una parcela | `200` |
| `POST` | `/parcels` | Registra una parcela | `201` + `Location` |
| `PUT` | `/parcels/{id}` | Reemplaza los datos de una parcela | `200` |
| `DELETE` | `/parcels/{id}` | Inactiva una parcela (soft-delete) sin áreas de fumigación | `204` |

`ParcelResource`:

```json
{
  "id": 10,
  "farmId": 1,
  "cropId": 3,
  "name": "Lote Norte",
  "areaHectares": 12.50,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-78.5,-0.2],[-78.49,-0.2],[-78.49,-0.19],[-78.5,-0.19],[-78.5,-0.2]]]}",
  "phenologicalStage": "Floracion",
  "isActive": true,
  "imageUrl": null
}
```

`POST`/`PUT` reciben `farmId`, `cropId`, `name`, `geometry`, `phenologicalStage` (opcional, máx. 60 caracteres) e `imageUrl` (opcional). El `farmId`
y el `cropId` deben existir (`404` con `FarmNotFound` / `CropNotFound`). `geometry` debe ser un
`Polygon` cerrado con al menos cuatro posiciones y coordenadas válidas; si no, se devuelve
`400` con el código de validación correspondiente. El área resultante debe ser mayor que cero y
como máximo 100 000 ha (`ParcelAreaOutOfRange`). `DELETE` desactiva lógicamente la parcela (`isActive = false`) manteniendo el histórico de misiones operativas, y responde `409 Conflict` con
`ParcelHasFumigationAreas` si la parcela todavía tiene áreas de fumigación.

## Áreas de fumigación — `/api/v1/fumigation-areas`

| Método | Ruta | Descripción | Éxito |
|---|---|---|---|
| `GET` | `/fumigation-areas` | Lista áreas; filtro opcional `parcelId` | `200` |
| `GET` | `/fumigation-areas/{id}` | Obtiene un área | `200` |
| `POST` | `/fumigation-areas` | Define un área dentro de una parcela | `201` + `Location` |
| `PUT` | `/fumigation-areas/{id}` | Redefine el polígono y la parcela | `200` |
| `DELETE` | `/fumigation-areas/{id}` | Elimina el área | `204` |

`FumigationAreaResource`:

```json
{
  "id": 5,
  "parcelId": 10,
  "areaHectares": 3.20,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-78.5,-0.2],[-78.495,-0.2],[-78.495,-0.195],[-78.5,-0.195],[-78.5,-0.2]]]}"
}
```

`POST`/`PUT` reciben `parcelId` y `geometry`. La parcela debe existir (`404` con
`ParcelNotFound`) y el área no puede superar el área de la parcela: en ese caso se devuelve
`422 Unprocessable Entity` con `FumigationAreaExceedsParcelArea`.

## Paginación

Las colecciones aceptan `?page=0&size=20`:

- Sin `page` ni `size`: se devuelve la colección completa (compatibilidad con el frontend).
- Con paginación: se devuelve la página solicitada y la cabecera `X-Total-Count` con el total
  de registros.
- `size` se limita a `1..100`; valores negativos de `page` se normalizan a `0`.

## Errores

Formato:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Farm with id 42 was not found.",
  "instance": "/api/v1/farms/42",
  "code": "FarmNotFound"
}
```

| `ErrorType` | HTTP |
|---|---|
| `Validation` | `400 Bad Request` |
| `NotFound` | `404 Not Found` |
| `Conflict` | `409 Conflict` |
| `BusinessRule` | `422 Unprocessable Entity` |
| `Unexpected` | `500 Internal Server Error` |

Códigos del bounded context: `FarmNameRequired`, `FarmNameTooLong`, `FarmLocationRequired`,
`FarmLocationTooLong`, `FarmOwnerInvalid`, `FarmImageUrlInvalid`, `FarmNotFound`,
`FarmHasParcels`, `CropNameRequired`, `CropNameTooLong`, `CropVarietyRequired`,
`CropVarietyTooLong`, `CropNotFound`, `CropAlreadyExists`, `CropHasParcels`,
`ParcelNameRequired`, `ParcelNameTooLong`, `ParcelImageUrlInvalid`, `ParcelAreaOutOfRange`,
`ParcelFarmInvalid`, `ParcelCropInvalid`, `ParcelNotFound`, `ParcelGeometryInvalid`,
`ParcelHasFumigationAreas`, `FumigationAreaHasNoArea`, `FumigationAreaParcelInvalid`,
`FumigationAreaNotFound`, `FumigationAreaExceedsParcelArea`, `PolygonGeoJsonRequired`,
`InvalidPolygonType`, `PolygonCoordinatesMissing`, `PolygonTooFewPoints`,
`PolygonRingNotClosed`, `CoordinateOutOfRange`, `PolygonGeoJsonInvalid`.

## Ejemplo completo

```bash
# 1. Crear la finca
curl -X POST http://localhost:5000/api/v1/farms \
  -H 'Content-Type: application/json' \
  -d '{"name":"Finca San Rafael","location":"Ica, Peru","ownerId":12}'

# 2. Crear el cultivo
curl -X POST http://localhost:5000/api/v1/crops \
  -H 'Content-Type: application/json' \
  -d '{"name":"Maiz","variety":"Amarillo"}'

# 3. Crear la parcela
curl -X POST http://localhost:5000/api/v1/parcels \
  -H 'Content-Type: application/json' \
  -d '{
    "farmId": 1,
    "cropId": 1,
    "name": "Lote Norte",
    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-78.5,-0.2],[-78.49,-0.2],[-78.49,-0.19],[-78.5,-0.19],[-78.5,-0.2]]]}"
  }'

# 4. Definir el área de fumigación
curl -X POST http://localhost:5000/api/v1/fumigation-areas \
  -H 'Content-Type: application/json' \
  -d '{
    "parcelId": 1,
    "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-78.5,-0.2],[-78.495,-0.2],[-78.495,-0.195],[-78.5,-0.195],[-78.5,-0.2]]]}"
  }'
```

