# Field Management — mapeo de persistencia

Motor: **MySQL 8**. Todas las tablas usan `utf8mb4_unicode_ci` y nombres en `snake_case`
plural (convención global de `ModelBuilderExtensions.UseSnakeCaseNamingConvention`).

> Estado del esquema: generado por la migración `InitialCreate`
> (`Agridron.Backend.API/Infrastructure/Persistence/EntityFrameworkCore/Migrations`).
> Las entidades de este bounded context todavía no implementan `IAuditableEntity`, por lo que
> **no existen columnas `created_at` / `updated_at`**. Cuando un agregado adopte auditoría, el
> interceptor `AuditableEntityInterceptor` las llenará automáticamente y se requerirá una nueva
> migración.

## Modelo relacional

```
farms 1 ──── * parcels * ──── 1 crops
                    │
                    └──── * fumigation_areas
```

- `farms.owner_id` referencia a un usuario de IAM, **sin FK** (bounded context distinto).
- `parcels.farm_id` → `farms.id`.
- `parcels.crop_id` → `crops.id`.
- `fumigation_areas.parcel_id` → `parcels.id`.

Las referencias son **solo índices, no claves foráneas**: el modelo de EF Core no declara
relaciones de navegación y la migración no crea restricciones `FOREIGN KEY`. La integridad
referencial se valida en la capa de aplicación (por ejemplo, antes de borrar una finca se
cuentan sus parcelas). Ver [ADR 0002](adrs/0002-mysql-with-native-geometry.md).

## Tablas

### `farms`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `name` | `VARCHAR(120)` | NOT NULL |
| `location` | `VARCHAR(200)` | NOT NULL |
| `owner_id` | `BIGINT` | NOT NULL, índice |
| `image_url` | `VARCHAR(500)` | NULL |

Índices: `ix_farms_owner_id (owner_id)`, `ix_farms_name (name)`.

### `crops`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `name` | `VARCHAR(80)` | NOT NULL |
| `variety` | `VARCHAR(80)` | NOT NULL |

Índice único: `ux_crops_name_variety (name, variety)`.

### `parcels`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `farm_id` | `BIGINT` | NOT NULL, índice |
| `crop_id` | `BIGINT` | NOT NULL, índice |
| `name` | `VARCHAR(120)` | NOT NULL |
| `geometry_json` | `LONGTEXT` | NOT NULL (GeoJSON canónico) |
| `area_hectares` | `DECIMAL(10,2)` | Generada `VIRTUAL`, NOT NULL |
| `image_url` | `VARCHAR(500)` | NULL |
| `geometry_native` | `GEOMETRY` (SRID 4326) | Generada `STORED`, NOT NULL |

Índices: `ix_parcels_farm_id`, `ix_parcels_crop_id`, `sx_parcels_geometry` (espacial).

### `fumigation_areas`

| Columna | Tipo | Restricciones |
|---|---|---|
| `id` | `BIGINT` | PK, autoincremento |
| `parcel_id` | `BIGINT` | NOT NULL, índice |
| `geometry_json` | `LONGTEXT` | NOT NULL (GeoJSON canónico) |
| `area_hectares` | `DECIMAL(10,2)` | Generada `VIRTUAL`, NOT NULL |
| `geometry_native` | `GEOMETRY` (SRID 4326) | Generada `STORED`, NOT NULL |

Índices: `ix_fumigation_areas_parcel_id`, `sx_fumigation_areas_geometry` (espacial).

## Geometría

`MySql.EntityFrameworkCore` no soporta NetTopologySuite, así que el dominio usa el value object
`GeoJsonPolygon` y lo persiste como texto. La migración agrega por SQL las columnas derivadas:

```sql
ALTER TABLE `parcels`
  ADD COLUMN `geometry_native` GEOMETRY
    GENERATED ALWAYS AS (ST_GeomFromGeoJSON(`geometry_json`)) STORED NOT NULL;
CREATE SPATIAL INDEX `sx_parcels_geometry` ON `parcels` (`geometry_native`);

ALTER TABLE `fumigation_areas`
  ADD COLUMN `geometry_native` GEOMETRY
    GENERATED ALWAYS AS (ST_GeomFromGeoJSON(`geometry_json`)) STORED NOT NULL;
CREATE SPATIAL INDEX `sx_fumigation_areas_geometry` ON `fumigation_areas` (`geometry_native`);
```

Detalles importantes:

- `ST_GeomFromGeoJSON` se usa con **un solo argumento**. MySQL 8 rechaza un segundo argumento
  numérico (`Incorrect option value`); la función ya devuelve geometrías con SRID 4326.
- `geometry_native` es **`STORED NOT NULL`** porque MySQL exige `NOT NULL` para indexar
  espacialmente y el índice `SPATIAL` requiere una columna almacenada.
- `area_hectares` la calcula MySQL a partir del GeoJSON:
  `ST_Area(ST_GeomFromGeoJSON(geometry_json)) / 10000`. El esquema real la crea como columna
  **`VIRTUAL`**. El dominio también valida el área antes de persistir; la columna es la fuente
  de verdad al leer desde la base de datos.
- `geometry_json` se escribe como GeoJSON canónico desde el value object, por lo que las
  columnas generadas siempre reciben un polígono válido.

## Conversiones EF Core

| Dominio | Persistencia |
|---|---|
| `FarmId`, `ParcelId`, `CropId`, `FumigationAreaId` | `long` (`ValueConverter`) |
| `FarmName`, `Location`, `CropName`, `Variety` | `string` |
| `GeoJsonPolygon` | `string` (`LONGTEXT`) |
| `AreaHectares` | `decimal` (`DECIMAL(10,2)`, calculada por la BD) |

`OwnerId` es un `long` plano, no un value object. `BaseRepository.FindByIdAsync` resuelve el
tipo del key desde el modelo y construye el value object antes de llamar a `FindAsync`; las
consultas que comparan IDs convierten primero el valor a su value object (no usan `Id.Value`
dentro de una expresión LINQ).

## Migraciones

La migración inicial vive en
`Agridron.Backend.API/Infrastructure/Persistence/EntityFrameworkCore/Migrations` y se llama
`InitialCreate`:

```bash
dotnet ef migrations add InitialCreate \
  --project Agridron.Backend.API \
  --output-dir Infrastructure/Persistence/EntityFrameworkCore/Migrations

dotnet ef database update --project Agridron.Backend.API
```

Como la migración crea columnas espaciales con SQL raw, **debe revisarse a mano** después de
generarla: EF Core no conoce `geometry_native`, `area_hectares` ni los índices `sx_*`.

`Program.cs` aplica `context.Database.Migrate()` al arrancar la API. Si la migración falla, el
error se registra como advertencia y la aplicación continúa; en producción conviene revisar los
logs del arranque para confirmar que el esquema quedó al día.

## Pruebas de persistencia

Las pruebas de integración de este bounded context están en
`tests/Agridron.Backend.API.Tests/FieldManagement/Infrastructure/Persistence` y se ejecutan
contra un MySQL real:

- Usan la base `agridron_test_db` y `AGRIDRON_TEST_CONNECTION` para sobreescribir la cadena de
  conexión (útil en CI).
- Se **omiten automáticamente** si no hay un servidor MySQL accesible (`MySqlFactAttribute`).
- El fixture aplica las migraciones y trunca las tablas antes de cada prueba.
- Cubren el modelo (tablas, conversiones, columnas calculadas e índices) y los cuatro
  repositorios, incluyendo la persistencia de geometrías y el SRID 4326.
