import type { QuestionType } from 'src/lib/api/exercises/exercisesTypes';
import type { LabelKey } from 'src/lib/translations/translationKeys';

/** Names of the five basic question types (LP-111). */
export const questionTypeLabels: Record<QuestionType, LabelKey> = {
    choice: 'labelQuestionTypeChoice',
    text: 'labelQuestionTypeText',
    cloze: 'labelQuestionTypeCloze',
    match: 'labelQuestionTypeMatch',
    flashcard: 'labelQuestionTypeFlashcard',
};

/** In the order of the editor's "add" buttons. */
export const questionTypes: readonly QuestionType[] = [
    'choice',
    'text',
    'cloze',
    'match',
    'flashcard',
];
