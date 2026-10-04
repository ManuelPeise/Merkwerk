# 005 – Exercise content as JSON columns with polymorphic C# types

- Status: Accepted (LP-110, 2026-10-04)
- Date: 2026-10-02
- Ticket: LP-002

## Context

There are many question types (multiple choice, cloze, marking, syllables, …) and more will come. Each has a different structure.

## Decision

`Question.Payload`, `Question.Solution` and generator parameters are stored in MySQL `JSON` columns and mapped to polymorphic C# types (`System.Text.Json` with type discriminator) via value converters. Published exercises are frozen as `ExerciseVersion` snapshots.

## Alternatives considered

- One table per question type – a migration for every new type.
- Generic key/value tables – hard to query and validate.

## Consequences

- A new question type needs no migration.
- No database queries into the JSON content are planned.
- Assignments point to a fixed version, so editing never changes running attempts.
- Implemented in LP-110: the draft keeps questions as rows (`Questions`, JSON `Payload`/`Solution`); publishing copies
  the whole exercise into one JSON column (`ExerciseVersions.Content`). MySQL reorders JSON keys, so the serializer
  options (`ExerciseJson.Options`) allow the type discriminator anywhere in an object.
