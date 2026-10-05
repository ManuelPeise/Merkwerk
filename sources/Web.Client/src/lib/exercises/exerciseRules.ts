import type { IArithmeticSettings } from 'src/lib/api/exercises/exercisesTypes';
import type { EditorQuestion } from 'src/lib/exercises/editorQuestion';
import { parseCloze, parseDecimal } from 'src/lib/exercises/editorQuestions';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** Limits of the backend (ExerciseRules.cs, ArithmeticGenerator.cs) - the server checks them again. */
export const exerciseRules = {
    titleMaxLength: 100,
    minGrade: 1,
    maxGrade: 4,
    maxQuestions: 50,
    maxTextLength: 500,
    minOptions: 2,
    maxOptions: 8,
    minPairs: 2,
    maxPairs: 8,
    maxGaps: 10,
    minNumberRange: 5,
    maxNumberRange: 1000,
    minRangeForTenTransition: 20,
    minTaskCount: 1,
    maxTaskCount: 50,
} as const;

const isFilled = (text: string): boolean =>
    text.trim().length > 0 && text.trim().length <= exerciseRules.maxTextLength;

const isNumber = (text: string): boolean => parseDecimal(text) !== null;

/** Problems of one question as translated messages; empty when it can be saved. */
export const validateQuestion = (question: EditorQuestion): NotificationKey[] => {
    const problems: NotificationKey[] = [];

    if (!isFilled(question.prompt)) {
        problems.push('notificationQuestionPromptRequired');
    }

    switch (question.type) {
        case 'choice':
            if (
                question.options.length < exerciseRules.minOptions ||
                question.options.length > exerciseRules.maxOptions ||
                !question.options.every(isFilled)
            ) {
                problems.push('notificationChoiceOptionsInvalid');
            }

            if (question.correctIndexes.length === 0) {
                problems.push('notificationChoiceCorrectRequired');
            }

            break;
        case 'text':
            if (
                question.acceptedAnswers.length === 0 ||
                !question.acceptedAnswers.every(isFilled)
            ) {
                problems.push('notificationAcceptedAnswersRequired');
            } else if (
                question.inputKind === 'number' &&
                !question.acceptedAnswers.every(isNumber)
            ) {
                problems.push('notificationAcceptedAnswersNotNumbers');
            }

            if (
                question.inputKind === 'number' &&
                question.numberTolerance.trim() !== '' &&
                (parseDecimal(question.numberTolerance) ?? -1) < 0
            ) {
                problems.push('notificationNumberToleranceInvalid');
            }

            break;
        case 'cloze': {
            const { gaps } = parseCloze(question.text);

            if (gaps.length === 0 || gaps.length > exerciseRules.maxGaps) {
                problems.push('notificationClozeGapsInvalid');
            } else if (!gaps.every((answers) => answers.length > 0 && answers.every(isFilled))) {
                problems.push('notificationClozeGapEmpty');
            }

            break;
        }
        case 'match':
            if (
                question.pairs.length < exerciseRules.minPairs ||
                question.pairs.length > exerciseRules.maxPairs ||
                !question.pairs.every((pair) => isFilled(pair.left) && isFilled(pair.right))
            ) {
                problems.push('notificationMatchPairsInvalid');
            }

            break;
        case 'flashcard':
            if (!isFilled(question.front) || !isFilled(question.back)) {
                problems.push('notificationFlashcardSidesRequired');
            }

            break;
    }

    return problems;
};

/** Problems of the generator settings; empty when they can be used. */
export const validateArithmeticSettings = (settings: IArithmeticSettings): NotificationKey[] => {
    const problems: NotificationKey[] = [];

    if (settings.operations.length === 0) {
        problems.push('notificationOperationsRequired');
    }

    if (
        !Number.isInteger(settings.numberRange) ||
        settings.numberRange < exerciseRules.minNumberRange ||
        settings.numberRange > exerciseRules.maxNumberRange
    ) {
        problems.push('notificationNumberRangeInvalid');
    }

    if (
        !Number.isInteger(settings.taskCount) ||
        settings.taskCount < exerciseRules.minTaskCount ||
        settings.taskCount > exerciseRules.maxTaskCount
    ) {
        problems.push('notificationTaskCountInvalid');
    }

    if (
        settings.tenTransition !== 'any' &&
        (settings.numberRange < exerciseRules.minRangeForTenTransition ||
            !settings.operations.some(
                (operation) => operation === 'add' || operation === 'subtract',
            ))
    ) {
        problems.push('notificationTenTransitionInvalid');
    }

    return problems;
};
