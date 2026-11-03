# 0003 — Result pattern en lugar de excepciones de negocio

- **Estado:** Aceptado
- **Fecha:** 2026-11-03
- **Decisores:** equipo de backend

## Contexto

El scaffold ya incluye `Result<T>` y un `Error` record. El proyecto de referencia usa el mismo
patrón. Los errores de negocio (finca inexistente, polígono inválido, área excedida) son flujos
esperados y no deberían costar una excepción ni un `500`.

## Opciones consideradas

| Opción | Ventajas | Desventajas |
|---|---|---|
| `Result<T>` con catálogo de errores | Flujos esperados explícitos; mapeo directo a Problem Details; testeable | Más verboso que lanzar excepciones |
| Excepciones de dominio | Menos código en el camino feliz | Coste de rendimiento y riesgo de `500` accidentales |

## Decisión

Los casos de uso devuelven `Result<T>` / `Result` con un valor de un `enum` de errores por
bounded context. Las excepciones se reservan para fallos de infraestructura o errores de
programación, que el middleware global traduce a Problem Details.

## Consecuencias

- **Positivas:** los controladores mapean errores a códigos HTTP sin `try/catch`.
- **Negativas:** los handlers deben propagar el `Result` explícitamente.
- **Seguimiento:** el mapeo `enum → HTTP status → título` vive en `Shared` y es común a todos
  los BCs.
