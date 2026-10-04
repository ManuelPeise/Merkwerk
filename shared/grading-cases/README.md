# Grading cases

JSON cases that define how answers are graded (LP-111). The C# graders in `Logic.Content/Grading` are authoritative;
a later client-side grader (TypeScript, offline mode – ADR 001) must pass the same files. `Logic.Content.Tests`
(`GradingCaseTests`) runs every file in CI.

## Layout

One folder per question type, named like the type discriminator (`QuestionTypes`): `choice/`, `text/`, `cloze/`,
`match/`, `flashcard/`. File names are kebab-case and say what the case shows.

## Shape

```json
{
  "description": "Two swapped letters within tolerance count as wrong but return the almost-right hint",
  "question": {
    "payload": { "type": "text", "prompt": "Übersetze: Katze", "inputKind": "text" },
    "solution": { "type": "text", "acceptedAnswers": ["cat"], "caseSensitive": false, "allowTypo": true }
  },
  "response": { "type": "text", "text": "cta" },
  "expected": { "isCorrect": false, "points": 0, "maxPoints": 1, "hint": "almostRight", "parts": [false] }
}
```

`question` is exactly what an exercise stores (ADR 005, `ExerciseJson.Options`), `response` what the child answered.

## Rules

- **Points:** gaps (cloze) and pairs (match) count one point each, `parts` says which were right; every other type is
  worth one point. `isCorrect` only when every part is right. Multiple choice is all or nothing.
- **Text:** trimmed, inner spaces joined, Unicode composed, `.`/`!`/`?` at the end ignored, case ignored unless
  `caseSensitive`. With `allowTypo`, one typo (missing, extra, replaced or swapped letter) is wrong with the hint
  `almostRight`.
- **Numbers** (`inputKind: number`): dot or comma as decimal separator, equal within `numberTolerance` (none = exact).
- **Cloze:** same text rules per gap, no typo tolerance.
- **Flashcard:** self-assessment, `knew: true` is right.
- A response of another question type is wrong (all parts false).

## Adding cases

- Cover ordinary behavior, empty and boundary inputs, and invalid or partial answers.
- Do not change an expected result only to make a failing test pass – confirm the intended rule first.
- At least 10 cases per type (flashcards: 3, there are only two outcomes).
