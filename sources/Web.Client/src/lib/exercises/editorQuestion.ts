import type { QuestionType, TextInputKind } from 'src/lib/api/exercises/exercisesTypes';

/** One answer pair of a match question: the right entry belongs to the left one. */
export interface IMatchPair {
    left: string;
    right: string;
}

interface IEditorQuestionBase {
    /** Stable React key while editing; never sent to the server. */
    key: string;
    type: QuestionType;
    prompt: string;
}

export interface IEditorChoiceQuestion extends IEditorQuestionBase {
    type: 'choice';
    options: string[];
    correctIndexes: number[];
    multipleAnswers: boolean;
}

export interface IEditorTextQuestion extends IEditorQuestionBase {
    type: 'text';
    inputKind: TextInputKind;
    acceptedAnswers: string[];
    caseSensitive: boolean;
    allowTypo: boolean;
    /** As typed (comma or dot); empty = exact. */
    numberTolerance: string;
}

/** Gaps written inline: "Der [Hund|Dackel] bellt." */
export interface IEditorClozeQuestion extends IEditorQuestionBase {
    type: 'cloze';
    text: string;
    caseSensitive: boolean;
}

export interface IEditorMatchQuestion extends IEditorQuestionBase {
    type: 'match';
    pairs: IMatchPair[];
}

export interface IEditorFlashcardQuestion extends IEditorQuestionBase {
    type: 'flashcard';
    front: string;
    back: string;
}

/** A question as the editor holds it - easier to edit than the API shape (see editorQuestions.ts). */
export type EditorQuestion =
    | IEditorChoiceQuestion
    | IEditorTextQuestion
    | IEditorClozeQuestion
    | IEditorMatchQuestion
    | IEditorFlashcardQuestion;
