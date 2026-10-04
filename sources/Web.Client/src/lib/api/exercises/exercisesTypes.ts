/** Mirrors the C# DTOs of ExercisesController (Web.Core); enums travel as camelCase strings (LP-110, LP-131). */
export type ExerciseContentSource = 'questions' | 'wordList' | 'generator';

export type ExerciseState = 'draft' | 'published' | 'archived';

export type QuestionType = 'choice' | 'text' | 'cloze' | 'match' | 'flashcard';

export type TextInputKind = 'text' | 'number';

export interface IChoicePayload {
    type: 'choice';
    prompt: string;
    options: string[];
    multipleAnswers: boolean;
}

export interface ITextPayload {
    type: 'text';
    prompt: string;
    inputKind: TextInputKind;
}

/** A gap sits between two neighbouring parts ("Der ", " bellt." = "Der ___ bellt."). */
export interface IClozePayload {
    type: 'cloze';
    prompt: string;
    parts: string[];
}

export interface IMatchPayload {
    type: 'match';
    prompt: string;
    left: string[];
    right: string[];
}

export interface IFlashcardPayload {
    type: 'flashcard';
    prompt: string;
    front: string;
}

export type QuestionPayload =
    | IChoicePayload
    | ITextPayload
    | IClozePayload
    | IMatchPayload
    | IFlashcardPayload;

export interface IChoiceSolution {
    type: 'choice';
    correctIndexes: number[];
}

export interface ITextSolution {
    type: 'text';
    acceptedAnswers: string[];
    caseSensitive: boolean;
    allowTypo: boolean;
    /** Allowed difference for numbers; null = exact. */
    numberTolerance: number | null;
}

export interface IClozeSolution {
    type: 'cloze';
    /** Accepted answers per gap. */
    gaps: string[][];
    caseSensitive: boolean;
}

export interface IMatchSolution {
    type: 'match';
    /** For every left entry the index of its right partner. */
    rightIndexForLeft: number[];
}

export interface IFlashcardSolution {
    type: 'flashcard';
    back: string;
}

export type QuestionSolution =
    | IChoiceSolution
    | ITextSolution
    | IClozeSolution
    | IMatchSolution
    | IFlashcardSolution;

export interface IQuestion {
    payload: QuestionPayload;
    solution: QuestionSolution;
}

export type ArithmeticOperation = 'add' | 'subtract' | 'multiply' | 'divide';

export type TenTransition = 'any' | 'without' | 'with';

export type PlaceholderMode = 'none' | 'mixed' | 'only';

/** Generator settings (LP-131); later more types (times table, LP-132). */
export interface IArithmeticSettings {
    type: 'arithmetic';
    operations: ArithmeticOperation[];
    numberRange: number;
    tenTransition: TenTransition;
    placeholder: PlaceholderMode;
    taskCount: number;
}

export type GeneratorSettings = IArithmeticSettings;

export interface IExerciseSummary {
    id: number;
    title: string;
    subjectId: number;
    grade: number;
    contentSource: ExerciseContentSource;
    state: ExerciseState;
    /** 0 = never published. */
    latestVersion: number;
    hasUnpublishedChanges: boolean;
    /** Questions, or the generator's task count. */
    questionCount: number;
}

export interface IExerciseDetail {
    exercise: IExerciseSummary;
    questions: IQuestion[];
    generator: GeneratorSettings | null;
}

export interface ISaveExerciseRequest {
    title: string;
    subjectId: number;
    grade: number;
    contentSource: ExerciseContentSource;
    questions: IQuestion[];
    generator: GeneratorSettings | null;
}

export interface IUpdateExerciseRequest {
    id: number;
    exercise: ISaveExerciseRequest;
}

export interface IExerciseIdRequest {
    id: number;
}

export interface IArchiveExerciseRequest {
    id: number;
    archived: boolean;
}

export interface IGeneratePreviewRequest {
    generator: GeneratorSettings;
    seed?: number;
}

export interface IGeneratorPreview {
    seed: number;
    questions: IQuestion[];
}
