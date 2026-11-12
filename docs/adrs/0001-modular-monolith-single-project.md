# 0001 — Monolito modular en un único proyecto

- **Estado:** Aceptado
- **Fecha:** 2026-11-03
- **Decisores:** equipo de backend

## Contexto

El informe del curso exige una arquitectura limpia y DDD con seis bounded contexts, pero el
equipo es pequeño, el plazo es corto y el despliegue será un único servicio en Render.
El scaffold inicial ya existe como un solo proyecto `Agridron.Backend.API` con un `Shared`
kernel y una convención de carpetas.

## Opciones consideradas

| Opción | Ventajas | Desventajas |
|---|---|---|
| Un proyecto con carpetas por BC | Simple de construir y desplegar; menos fricción; coincide con el scaffold | Las fronteras dependen de la disciplina, no del compilador |
| Multi-proyecto por capa | Fronteras verificadas por el compilador | Cuatro csproj por BC, más tiempo de setup y de CI |
| Multi-proyecto por BC | Máximo aislamiento | Sobredimensionado para el alcance del curso |

## Decisión

Un único proyecto `Agridron.Backend.API` con un directorio por bounded context, cada uno con
las capas `Domain`, `Application`, `Infrastructure` e `Interfaces`. Las fronteras se protegen
por convención, revisión de código y la regla de dependencia documentada.

## Consecuencias

- **Positivas:** build y despliegue simples; un solo `AppDbContext`; migraciones centralizadas.
- **Negativas:** nada impide que un BC referencie a otro por error. Se mitiga con revisión y
  con la regla de “solo IDs y eventos de integración entre BCs”.
- **Seguimiento:** si el equipo crece o un BC necesita escalar por separado, se evalúa extraerlo
  a un proyecto propio en un ADR nuevo.
