# 0002 — MySQL 8 con columna espacial nativa

- **Estado:** Aceptado
- **Fecha:** 2026-11-03
- **Decisores:** equipo de backend

## Contexto

El informe exige persistencia relacional/espacial en **MySQL** con EF Core. El proveedor
instalado es `MySql.EntityFrameworkCore` (Oracle), que **no** implementa NetTopologySuite.
La alternativa sería migrar a `Pomelo.EntityFrameworkCore.MySql`, cuyo soporte para EF Core 10
no está garantizado al momento de escribir esta decisión.

Las parcelas se representan como GeoJSON (`Polygon`) y el frontend ya envía ese string.

## Opciones consideradas

| Opción | Ventajas | Desventajas |
|---|---|---|
| GeoJSON en `LONGTEXT` + columna generada `GEOMETRY SRID 4326` | Cumple persistencia espacial real; cero riesgo de build; índice espacial disponible | Las consultas espaciales se escriben en SQL raw |
| Pomelo + NetTopologySuite | LINQ espacial completo | Riesgo de incompatibilidad con EF Core 10 |
| Solo GeoJSON en texto | Lo más simple | No cumple el requisito de persistencia espacial |

## Decisión

Mantener `MySql.EntityFrameworkCore`. El GeoJSON se guarda en `geometry_json` (`LONGTEXT`) y la
migración inicial agrega una columna generada y su índice espacial:

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

`ST_GeomFromGeoJSON` se invoca con un solo argumento: MySQL 8 rechaza el segundo argumento
numérico y la función ya devuelve geometrías con SRID 4326. La columna se declara `STORED NOT
NULL` porque el índice `SPATIAL` exige una columna almacenada y no nula.

## Consecuencias

- **Positivas:** el contrato con el frontend no cambia; hay índice espacial; el build es estable.
- **Negativas:** las consultas de intersección/vecindad usan SQL raw o `FromSqlRaw`.
- **Seguimiento:** si Pomelo publica soporte estable para EF Core 10, se evalúa migrar en un ADR
  nuevo sin cambiar el modelo de dominio.
