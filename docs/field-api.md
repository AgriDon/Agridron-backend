# Field Management — contrato HTTP

Base URL: `http://localhost:5000/api/v1`

Todos los recursos usan **camelCase** en JSON. Los errores usan **RFC 7807** (`application/problem+json`).
Las colecciones se devuelven como array plano e incluyen la cabecera `X-Total-Count`.

Unidades: `area` se expresa en **hectáreas (ha)**. `geometry` es un **string** con GeoJSON
(tipo `Polygon`), tal como lo envía el frontend.

---

## Farms

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/farms` | Lista fincas. Acepta `?page=&size=&ownerId=` |
| `GET` | `/farms/{id}` | Obtiene una finca |
| `POST` | `/farms` | Crea una finca |
| `PUT` | `/farms/{id}` | Actualiza una finca |
| `DELETE` | `/farms/{id}` | Elimina una finca sin parcelas |

### FarmResource

```json
{
  "id": 1,
  "name": "Finca San Rafael",
  "location": "Ica, Peru",
  "ownerId": 1,
  "image": "https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=600"
}
```

### CreateFarmResource

```json
{
  "name": "Finca San Rafael",
  "location": "Ica, Peru",
  "ownerId": 1,
  "image": "https://example.com/farm.jpg"
}
```

Reglas: `name` obligatorio (1–120), `location` obligatorio (1–200), `ownerId` > 0,
`image` opcional (URL, máximo 500 caracteres).

`DELETE` devuelve `409 Conflict` si la finca tiene parcelas.

---

## Parcels

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/parcels` | Lista parcelas. Acepta `?farmId=&cropId=&page=&size=` |
| `GET` | `/parcels/{id}` | Obtiene una parcela |
| `POST` | `/parcels` | Crea una parcela |
| `PUT` | `/parcels/{id}` | Actualiza una parcela |
| `DELETE` | `/parcels/{id}` | Elimina una parcela sin áreas de fumigación |

### ParcelResource

```json
{
  "id": 1,
  "name": "Parcela Norte",
  "area": 12.5,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-76.14,-14.07],[-76.13,-14.07],[-76.13,-14.06],[-76.14,-14.06],[-76.14,-14.07]]]}",
  "farmId": 1,
  "cropId": 1,
  "image": "https://images.unsplash.com/photo-1500937386664-56d1dfef3854?w=600"
}
```

### CreateParcelResource

```json
{
  "name": "Parcela Norte",
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-76.14,-14.07],[-76.13,-14.07],[-76.13,-14.06],[-76.14,-14.06],[-76.14,-14.07]]]}",
  "farmId": 1,
  "cropId": 1,
  "image": "https://example.com/parcel.jpg"
}
```

`area` es **calculada por el backend** a partir de `geometry`; si el cliente la envía, se ignora.
Reglas: polígono válido (ver abajo), `farmId` y `cropId` existentes.

### Validación de polígono

- JSON válido con `type = "Polygon"`.
- `coordinates` es un array con al menos un anillo.
- El primer anillo (exterior) tiene al menos 4 posiciones y está cerrado (primera = última).
- Cada posición es `[longitud, latitud]` con longitud ∈ [-180, 180] y latitud ∈ [-90, 90].
- El área calculada debe ser mayor que 0 y menor o igual a 100 000 ha.

---

## Crops

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/crops` | Lista cultivos |
| `GET` | `/crops/{id}` | Obtiene un cultivo |
| `POST` | `/crops` | Crea un cultivo |
| `PUT` | `/crops/{id}` | Actualiza un cultivo |
| `DELETE` | `/crops/{id}` | Elimina un cultivo sin parcelas |

### CropResource

```json
{
  "id": 1,
  "name": "Maiz",
  "variety": "Ambar"
}
```

Reglas: `name` obligatorio (1–80), `variety` obligatorio (1–80). La combinación
`name + variety` es única (`409 Conflict` si se repite).

---

## Fumigation areas

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/fumigation-areas` | Lista áreas. Acepta `?parcelId=` |
| `GET` | `/fumigation-areas/{id}` | Obtiene un área |
| `POST` | `/fumigation-areas` | Crea un área |
| `PUT` | `/fumigation-areas/{id}` | Actualiza un área |
| `DELETE` | `/fumigation-areas/{id}` | Elimina un área |

### FumigationAreaResource

```json
{
  "id": 1,
  "parcelId": 1,
  "geometry": "{\"type\":\"Polygon\",\"coordinates\":[[[-76.135,-14.065],[-76.133,-14.065],[-76.133,-14.063],[-76.135,-14.063],[-76.135,-14.065]]]}",
  "area": 3.2
}
```

Reglas: `parcelId` existente; polígono válido; `area` calculada ≤ área de la parcela
(si se excede → `422 Unprocessable Entity`).

---

## Catálogo de errores

| `code` | HTTP | Significado |
|---|---|---|
| `FarmNotFound` | 404 | La finca no existe |
| `ParcelNotFound` | 404 | La parcela no existe |
| `CropNotFound` | 404 | El cultivo no existe |
| `FumigationAreaNotFound` | 404 | El área de fumigación no existe |
| `FarmNameRequired` | 400 | El nombre de la finca está vacío |
| `FarmLocationRequired` | 400 | La ubicación está vacía |
| `FarmOwnerRequired` | 400 | `ownerId` inválido |
| `CropNameRequired` | 400 | El nombre del cultivo está vacío |
| `CropVarietyRequired` | 400 | La variedad está vacía |
| `CropAlreadyExists` | 409 | Ya existe `name + variety` |
| `ParcelNameRequired` | 400 | El nombre de la parcela está vacío |
| `ParcelGeometryInvalid` | 400 | El GeoJSON no es un polígono válido |
| `ParcelAreaOutOfRange` | 400 | El área calculada no es razonable |
| `FarmHasParcels` | 409 | No se puede borrar una finca con parcelas |
| `CropHasParcels` | 409 | No se puede borrar un cultivo en uso |
| `ParcelHasFumigationAreas` | 409 | No se puede borrar una parcela con áreas |
| `FumigationAreaExceedsParcelArea` | 422 | El área supera la de la parcela |
| `InvalidPolygonType` | 400 | `type` distinto de `Polygon` |
| `PolygonRingNotClosed` | 400 | El anillo no cierra |
| `PolygonTooFewPoints` | 400 | Menos de 4 posiciones |
| `CoordinateOutOfRange` | 400 | Latitud/longitud fuera de rango |

### Ejemplo de error

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "The fumigation area exceeds the parcel area.",
  "instance": "/api/v1/fumigation-areas",
  "code": "FumigationAreaExceedsParcelArea"
}
```
