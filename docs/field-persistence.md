# Field Management — mapeo de persistencia

Motor: **MySQL 8**. Todas las tablas usan `utf8mb4_unicode_ci` y nombres en `snake_case`
plural (convención global de `ModelBuilderExtensions.UseSnakeCaseNamingConvention`).
Fechas en UTC.

## Modelo relacional

```
farms 1 ──── * parcels * ──── 1 crops
                    │
                    └──── * fumigation_areas
```

- `farms.owner_id` referencia a un usuario de IAM, **sin FK** (bounded context distinto).
- `parcels.farm_id` → `farms.id` (`ON DELETE RESTRICT`).
- `parcels.crop_id` → `crops.id` (`ON DELETE RESTRICT`).
- `fumigation_areas.parcel_id` → `parcels.id` (`ON DELETE RESTRICT`).

## Tablas

### `farms`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `name` | `VARCHAR(120)` | NOT NULL |
| `location` | `VARCHAR(200)` | NOT NULL |
| `owner_id` | `BIGINT` | NOT NULL, índice |
| `image_url` | `VARCHAR(500)` | NULL |
| `created_at` | `DATETIME(6)` | NOT NULL |
| `updated_at` | `DATETIME(6)` | NOT NULL |

Índices: `ix_farms_owner_id (owner_id)`, `ix_farms_name (name)`.

### `crops`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `name` | `VARCHAR(80)` | NOT NULL |
| `variety` | `VARCHAR(80)` | NOT NULL |
| `created_at` | `DATETIME(6)` | NOT NULL |
| `updated_at` | `DATETIME(6)` | NOT NULL |

Índice único: `ux_crops_name_variety (name, variety)`.

### `parcels`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `name` | `VARCHAR(120)` | NOT NULL |
| `area_hectares` | `DECIMAL(12,2)` | NOT NULL, > 0 |
| `geometry` | `LONGTEXT` | NOT NULL (GeoJSON) |
| `geom` | `GEOMETRY SRID 4326` | Generada `STORED` desde `geometry` |
| `farm_id` | `BIGINT` | FK → `farms.id`, NOT NULL |
| `crop_id` | `BIGINT` | FK → `crops.id`, NOT NULL |
| `image_url` | `VARCHAR(500)` | NULL |
| `created_at` | `DATETIME(6)` | NOT NULL |
| `updated_at` | `DATETIME(6)` | NOT NULL |

Índices: `ix_parcels_farm_id`, `ix_parcels_crop_id`, `SPATIAL ix_parcels_geom (geom)`.

La columna `geom` se agrega por SQL en la migración:

```sql
ALTER TABLE parcels
  ADD COLUMN geom GEOMETRY SRID 4326
    AS (ST_GeomFromGeoJSON(geometry)) STORED,
  ADD SPATIAL INDEX ix_parcels_geom (geom);
```

> Nota: `MySql.EntityFrameworkCore` no soporta NetTopologySuite. El dominio usa un value object
> `GeoJsonPolygon` que se persiste como string. Ver [ADR 0002](adrs/0002-mysql-with-native-geometry.md).

### `fumigation_areas`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `parcel_id` | `BIGINT` | FK → `parcels.id`, NOT NULL |
| `geometry` | `LONGTEXT` | NOT NULL (GeoJSON) |
| `geom` | `GEOMETRY SRID 4326` | Generada `STORED` |
| `area_hectares` | `DECIMAL(12,2)` | NOT NULL, > 0 |
| `created_at` | `DATETIME(6)` | NOT NULL |
| `updated_at` | `DATETIME(6)` | NOT NULL |

Índices: `ix_fumigation_areas_parcel_id`, `SPATIAL ix_fumigation_areas_geom (geom)`.

## Conversiones EF Core

| Dominio | Persistencia |
|---|---|
| `FarmId`, `ParcelId`, `CropId`, `FumigationAreaId` | `long` (`HasConversion`) |
| `FarmName`, `Location` | `string` (owned type o `HasConversion`) |
| `GeoJsonPolygon` | `string` (`LONGTEXT`) |
| `Area` | `decimal` (`DECIMAL(12,2)`) |

## Migraciones

```bash
dotnet ef migrations add InitialFieldManagement \
  --project Agridron.Backend.API \
  --output-dir FieldManagement/Infrastructure/Persistence/EntityFrameworkCore/Migrations

dotnet ef migrations add AddFieldManagementSpatialColumns \
  --project Agridron.Backend.API \
  --output-dir FieldManagement/Infrastructure/Persistence/EntityFrameworkCore/Migrations

dotnet ef database update --project Agridron.Backend.API
```

## Seed

`FieldManagementSeeder` inserta los datos del mock de forma idempotente (solo si la tabla está
vacía): 6 fincas, 4 cultivos, 5 parcelas y 2 áreas de fumigación, con las mismas URLs de imagen
que usa el frontend en Sprint 2. Las coordenadas del mock se normalizan a polígonos válidos
y cerrados, porque una de las áreas de fumigación de ejemplo es degenerada (cuatro puntos
idénticos) y no pasaría la validación del dominio.
