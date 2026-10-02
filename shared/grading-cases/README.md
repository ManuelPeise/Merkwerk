# Grading cases

Data-driven test cases for every grader (`IGrader`) in `Logic.Shared`.
`Logic.Shared.Tests` loads every `*.json` file below this folder and checks the grader's result.
The same files can later be used to test a TypeScript grader (Expo app) – one source of truth for "what is correct".

## Layout

```
shared/grading-cases/
  <question-type-kebab>/        e.g. multiple-choice, free-text, cloze, matching, marking, syllables
    <case-name>.json            one case per file, name describes the case
```

## File format

```json
{
  "description": "Upper/lower case is ignored when the list says so",
  "questionType": "free-text",
  "payload":  { "prompt": "Translate: Hund" },
  "solution": { "accepted": ["dog"], "caseSensitive": false, "typoTolerance": 1 },
  "response": { "text": "Dog" },
  "expected": { "isCorrect": true, "points": 1, "maxPoints": 1, "hint": null }
}
```

| Field | Meaning |
| --- | --- |
| `description` | What the case proves – shown as test name |
| `questionType` | Type discriminator, same value as in `[JsonDerivedType]` |
| `payload` / `solution` | Exactly as stored in `Question.Payload` / `Question.Solution` |
| `response` | The child's answer (`AnswerResponse`) |
| `expected` | `GradingResult`: correct?, points, max points, optional hint key (e.g. `"almost-right"`) |

## Rules

- At least 10 cases per question type, including edge cases: empty answer, whitespace, upper/lower case,
  typos within and beyond tolerance, partial answers, several accepted answers.
- Never change an existing case to make a test pass – add a new one and discuss the behaviour change.
