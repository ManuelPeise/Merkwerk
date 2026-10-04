import type { IQuestion, QuestionType } from 'src/lib/api/exercises/exercisesTypes';
import type { EditorQuestion } from 'src/lib/exercises/editorQuestion';

let lastKey = 0;

/** Keys only have to be unique while the page is open. */
const newKey = (): string => {
    lastKey += 1;
    return `question-${lastKey}`;
};

const gapPattern = /\[([^\]]*)\]/g;

/** "Der [Hund|Dackel] bellt." -> parts ["Der ", " bellt."], gaps [["Hund", "Dackel"]]. */
export const parseCloze = (text: string): { parts: string[]; gaps: string[][] } => {
    const parts: string[] = [];
    const gaps: string[][] = [];
    let position = 0;

    for (const match of text.matchAll(gapPattern)) {
        parts.push(text.slice(position, match.index));
        gaps.push(match[1].split('|').map((answer) => answer.trim()));
        position = match.index + match[0].length;
    }

    parts.push(text.slice(position));
    return { parts, gaps };
};

/** The reverse of parseCloze. */
export const formatCloze = (parts: string[], gaps: string[][]): string =>
    parts
        .map((part, index) => (index < gaps.length ? `${part}[${gaps[index].join('|')}]` : part))
        .join('');

/** Reads "0,5" and "0.5"; null when empty or no number. */
export const parseDecimal = (value: string): number | null => {
    const normalized = value.trim().replace(',', '.');
    return normalized === '' || Number.isNaN(Number(normalized)) ? null : Number(normalized);
};

export const createQuestion = (type: QuestionType): EditorQuestion => {
    const key = newKey();

    switch (type) {
        case 'choice':
            return {
                key,
                type,
                prompt: '',
                options: ['', ''],
                correctIndexes: [],
                multipleAnswers: false,
            };
        case 'text':
            return {
                key,
                type,
                prompt: '',
                inputKind: 'text',
                acceptedAnswers: [''],
                caseSensitive: false,
                allowTypo: true,
                numberTolerance: '',
            };
        case 'cloze':
            return { key, type, prompt: '', text: '', caseSensitive: false };
        case 'match':
            return {
                key,
                type,
                prompt: '',
                pairs: [
                    { left: '', right: '' },
                    { left: '', right: '' },
                ],
            };
        case 'flashcard':
            return { key, type, prompt: '', front: '', back: '' };
    }
};

/** API shape -> editor shape. Match pairs are lined up by the solution. */
export const toEditorQuestion = (question: IQuestion): EditorQuestion => {
    const { payload, solution } = question;
    const key = newKey();

    if (payload.type === 'choice' && solution.type === 'choice') {
        return {
            key,
            type: 'choice',
            prompt: payload.prompt,
            options: payload.options,
            correctIndexes: solution.correctIndexes,
            multipleAnswers: payload.multipleAnswers,
        };
    }

    if (payload.type === 'text' && solution.type === 'text') {
        return {
            key,
            type: 'text',
            prompt: payload.prompt,
            inputKind: payload.inputKind,
            acceptedAnswers: solution.acceptedAnswers,
            caseSensitive: solution.caseSensitive,
            allowTypo: solution.allowTypo,
            numberTolerance:
                solution.numberTolerance === null ? '' : String(solution.numberTolerance),
        };
    }

    if (payload.type === 'cloze' && solution.type === 'cloze') {
        return {
            key,
            type: 'cloze',
            prompt: payload.prompt,
            text: formatCloze(payload.parts, solution.gaps),
            caseSensitive: solution.caseSensitive,
        };
    }

    if (payload.type === 'match' && solution.type === 'match') {
        return {
            key,
            type: 'match',
            prompt: payload.prompt,
            pairs: payload.left.map((left, index) => ({
                left,
                right: payload.right[solution.rightIndexForLeft[index]] ?? '',
            })),
        };
    }

    if (payload.type === 'flashcard' && solution.type === 'flashcard') {
        return {
            key,
            type: 'flashcard',
            prompt: payload.prompt,
            front: payload.front,
            back: solution.back,
        };
    }

    throw new Error(`Payload and solution of different types: ${payload.type}/${solution.type}`);
};

/** Editor shape -> API shape. Texts are trimmed; match pairs are stored in row order. */
export const toQuestion = (question: EditorQuestion): IQuestion => {
    const prompt = question.prompt.trim();

    switch (question.type) {
        case 'choice':
            return {
                payload: {
                    type: 'choice',
                    prompt,
                    options: question.options.map((option) => option.trim()),
                    multipleAnswers: question.multipleAnswers,
                },
                solution: {
                    type: 'choice',
                    correctIndexes: [...question.correctIndexes].sort((a, b) => a - b),
                },
            };
        case 'text':
            return {
                payload: { type: 'text', prompt, inputKind: question.inputKind },
                solution: {
                    type: 'text',
                    acceptedAnswers: question.acceptedAnswers.map((answer) => answer.trim()),
                    caseSensitive: question.caseSensitive,
                    allowTypo: question.inputKind === 'text' && question.allowTypo,
                    numberTolerance:
                        question.inputKind === 'number'
                            ? parseDecimal(question.numberTolerance)
                            : null,
                },
            };
        case 'cloze': {
            const { parts, gaps } = parseCloze(question.text);
            return {
                payload: { type: 'cloze', prompt, parts },
                solution: { type: 'cloze', gaps, caseSensitive: question.caseSensitive },
            };
        }
        case 'match':
            return {
                payload: {
                    type: 'match',
                    prompt,
                    left: question.pairs.map((pair) => pair.left.trim()),
                    right: question.pairs.map((pair) => pair.right.trim()),
                },
                solution: {
                    type: 'match',
                    rightIndexForLeft: question.pairs.map((_, index) => index),
                },
            };
        case 'flashcard':
            return {
                payload: { type: 'flashcard', prompt, front: question.front.trim() },
                solution: { type: 'flashcard', back: question.back.trim() },
            };
    }
};
