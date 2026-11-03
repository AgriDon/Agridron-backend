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

Mantener `MySql.EntityFrameworkCore`. El GeoJSON se guarda en `parcels.geometry` (`LONGTEXT`)
y una migración agrega una columna generada:

```sql
ALTER TABLE parcels
  ADD COLUMN geom GEOMETRY SRID 4326
    AS (ST_GeomFromGeoJSON(geometry)) STORED,
  ADD SPATIAL INDEX ix_parcels_geom (geom);
```

## Consecuencias

- **Positivas:** el contrato con el frontend no cambia; hay índice espacial; el build es estable.
- **Negativas:** las consultas de intersección/vecindad usan SQL raw o `FromSqlRaw`.
- **Seguimiento:** si Pomelo publica soporte estable para EF Core 10, se evalúa migrar en un ADR
  nuevo sin cambiar el modelo de dominio.
