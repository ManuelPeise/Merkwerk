# Grading cases

This directory holds JSON examples for grader behavior shared between the server and client. At present it contains
only two `free-text` examples; it is an initial fixture set, not a complete grader specification.

## Current files

```text
shared/grading-cases/
└─ free-text/
   ├─ case-insensitive-match.json
   └─ one-typo-is-almost-right.json
```

## Fixture shape

The examples describe a question type, payload, solution, response, and expected grading result. Use the existing cases
as the source of truth for the exact current JSON shape. As additional question types and graders are implemented,
their fixtures should be added in a matching kebab-case directory.

## Adding cases

- Give each fixture a descriptive filename and explain the behavior it covers.
- Add cases for ordinary behavior, empty and boundary inputs, and invalid or partial answers as appropriate to the
  grader.
- Do not edit an existing expected result only to make a failing test pass. Confirm the intended grading behavior and
  add or update a case deliberately.
- A fixture does not by itself mean a grader or automated test runner exists. Add or update the corresponding tests
  when implementing a grader.
