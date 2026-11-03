# Architecture Decision Records

Registro de las decisiones de arquitectura del backend de AgriDron.
Cada ADR documenta el contexto, la decisión y sus consecuencias. Las decisiones no se borran:
si cambian, se crea un ADR nuevo que reemplaza al anterior.

| ADR | Título | Estado |
|---|---|---|
| [0001](0001-modular-monolith-single-project.md) | Monolito modular en un único proyecto | Aceptado |
| [0002](0002-mysql-with-native-geometry.md) | MySQL 8 con columna espacial nativa | Aceptado |
| [0003](0003-result-pattern-over-exceptions.md) | Result pattern en lugar de excepciones de negocio | Aceptado |

## Cómo agregar un ADR

1. Copia [`template.md`](template.md).
2. Nómbralo `NNNN-titulo-en-kebab-case.md` con el siguiente número disponible.
3. Complétalo y agrégalo a la tabla.
4. Commit: `docs(adrs): add adr NNNN <titulo>`.
