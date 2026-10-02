# LP-009 – Click dummy test with the children (Gate 1)

Goal: before building the children's client (LP-116 ff.), check with both children (aged 6 and 9) that they get
through the core loop **without help**: choose profile → see tasks → solve a task → feedback → stars.

Click dummy: Claude design canvas "Merkwerk Design-Richtungen", artboard "LP-009 · Klickdummy (Richtung A)",
started with **Play** on the Android tablet. It uses design direction A and the values from `shared/design-tokens`.
No code in this repository – the dummy is thrown away after the test.

Content of the dummy:

- Profile selection with two cards (names set in the canvas tweaks)
- "Meine Aufgaben": maths (plus up to 20, on-screen keypad, 5 questions), English (word → choose the German meaning,
  3 questions), German (count syllables, 4 words)
- Feedback: ✓ "Super, richtig!" with "Weiter", or ↺ "Fast! Probier es nochmal." – never colour alone
- Progress dots, "Super gemacht!" screen with the stars earned, "Fertig" badge on finished tasks
- Read-aloud buttons (device voice via Web Speech API)

## How to run the test

1. One child at a time, the other one not watching.
2. Say only: "Das ist eine Lern-App. Probier mal aus." – then watch, don't explain.
3. Help only after about 20 seconds of being stuck; note where.
4. Afterwards ask: "Was war schön? Was war doof? Was würdest du ändern?"

## Observations

| Observation | Child 6 | Child 9 |
| --- | --- | --- |
| Found own profile without help | | |
| Started a task without help | | |
| Understood the keypad (delete, check mark) | | |
| Understood the feedback (✓ / ↺ try again) | | |
| Used / needed the read-aloud button | | |
| Found the way back to "Meine Aufgaben" | | |
| Hesitated or tapped wrongly where? | | |
| Liked / disliked (quotes) | | |
| **Got through without help (Gate 1)** | | |

## Result and changes

- Gate 1: passed / not passed
- Changes for the children's client (feed into LP-116 – LP-121):
  - …
