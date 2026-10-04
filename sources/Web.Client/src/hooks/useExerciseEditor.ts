import React from 'react';
import { exercisesApi } from 'src/lib/api/exercises/exercisesApi';
import type {
    IArithmeticSettings,
    IExerciseSummary,
    ISaveExerciseRequest,
    QuestionType,
} from 'src/lib/api/exercises/exercisesTypes';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { subjectsApi } from 'src/lib/api/subjects/subjectsApi';
import type { ISubject } from 'src/lib/api/subjects/subjectsTypes';
import type { EditorQuestion } from 'src/lib/exercises/editorQuestion';
import { createQuestion, toEditorQuestion, toQuestion } from 'src/lib/exercises/editorQuestions';
import {
    exerciseRules,
    validateArithmeticSettings,
    validateQuestion,
} from 'src/lib/exercises/exerciseRules';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** Content of the editor: only hand-written questions or a generator (word lists come with LP-140). */
export type EditorContentSource = 'questions' | 'generator';

export interface IExerciseForm {
    title: string;
    subjectId: number | null;
    grade: number | null;
    contentSource: EditorContentSource;
    questions: EditorQuestion[];
    generator: IArithmeticSettings;
}

export type ExerciseFieldErrors = Partial<Record<'title' | 'subjectId' | 'grade', NotificationKey>>;

export const defaultArithmeticSettings: IArithmeticSettings = {
    type: 'arithmetic',
    operations: ['add'],
    numberRange: 20,
    tenTransition: 'any',
    placeholder: 'none',
    taskCount: 10,
};

const emptyForm: IExerciseForm = {
    title: '',
    subjectId: null,
    grade: null,
    contentSource: 'questions',
    questions: [],
    generator: defaultArithmeticSettings,
};

/** "questions[2]" -> 2 */
const questionIndexPattern = /^questions\[(\d+)\]$/;

interface IExerciseEditor {
    form: IExerciseForm;
    /** Null until the exercise exists on the server. */
    summary: IExerciseSummary | null;
    subjects: ISubject[] | null;
    isLoading: boolean;
    isSaving: boolean;
    /** Changes since loading or the last save. */
    isDirty: boolean;
    fieldErrors: ExerciseFieldErrors;
    /** Problems per question key. */
    questionErrors: Record<string, NotificationKey[]>;
    generatorErrors: NotificationKey[];
    errorKey: NotificationKey | null;
    successKey: NotificationKey | null;
    update: (change: Partial<IExerciseForm>) => void;
    addQuestion: (type: QuestionType) => void;
    updateQuestion: (question: EditorQuestion) => void;
    moveQuestion: (key: string, direction: -1 | 1) => void;
    removeQuestion: (key: string) => void;
    /** Saves the draft; returns the exercise id, or null when something is wrong. */
    save: () => Promise<number | null>;
    /** Saves (if needed) and publishes; returns the exercise id, or null when something is wrong. */
    publish: () => Promise<number | null>;
}

/** State and actions of the exercise editor (LP-112). exerciseId null = new exercise. */
export const useExerciseEditor = (exerciseId: number | null): IExerciseEditor => {
    const [form, setForm] = React.useState<IExerciseForm>(emptyForm);
    const [summary, setSummary] = React.useState<IExerciseSummary | null>(null);
    const [subjects, setSubjects] = React.useState<ISubject[] | null>(null);
    const [isLoading, setIsLoading] = React.useState(exerciseId !== null);
    const [isSaving, setIsSaving] = React.useState(false);
    const [isDirty, setIsDirty] = React.useState(false);
    const [fieldErrors, setFieldErrors] = React.useState<ExerciseFieldErrors>({});
    const [questionErrors, setQuestionErrors] = React.useState<Record<string, NotificationKey[]>>(
        {},
    );
    const [generatorErrors, setGeneratorErrors] = React.useState<NotificationKey[]>([]);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [successKey, setSuccessKey] = React.useState<NotificationKey | null>(null);

    // The exercise that is already in the form - after creating one, the new URL must not reload it.
    const loadedId = React.useRef<number | null>(null);

    React.useEffect(() => {
        const controller = new AbortController();

        void subjectsApi.list.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind !== 'canceled') {
                setSubjects(result.data ?? []);
            }
        });

        return () => controller.abort();
    }, []);

    React.useEffect(() => {
        if (exerciseId === null || loadedId.current === exerciseId) {
            return;
        }

        const controller = new AbortController();

        void exercisesApi.get
            .get({ params: { id: exerciseId }, signal: controller.signal })
            .then((result) => {
                if (result.error?.kind === 'canceled') {
                    return;
                }

                setIsLoading(false);

                if (!result.data) {
                    setErrorKey(result.error?.messageKey ?? 'notificationUnexpectedError');
                    return;
                }

                const { exercise, questions, generator } = result.data;
                loadedId.current = exercise.id;
                setSummary(exercise);
                setForm({
                    title: exercise.title,
                    subjectId: exercise.subjectId,
                    grade: exercise.grade,
                    contentSource:
                        exercise.contentSource === 'generator' ? 'generator' : 'questions',
                    questions: questions.map(toEditorQuestion),
                    generator: generator ?? defaultArithmeticSettings,
                });
            });

        return () => controller.abort();
    }, [exerciseId]);

    const change = (next: (current: IExerciseForm) => IExerciseForm) => {
        setForm(next);
        setIsDirty(true);
        setSuccessKey(null);
    };

    const update = (partial: Partial<IExerciseForm>) =>
        change((current) => ({ ...current, ...partial }));

    const addQuestion = (type: QuestionType) =>
        change((current) => ({
            ...current,
            questions: [...current.questions, createQuestion(type)],
        }));

    const updateQuestion = (question: EditorQuestion) =>
        change((current) => ({
            ...current,
            questions: current.questions.map((q) => (q.key === question.key ? question : q)),
        }));

    const moveQuestion = (key: string, direction: -1 | 1) =>
        change((current) => {
            const index = current.questions.findIndex((q) => q.key === key);
            const target = index + direction;

            if (index < 0 || target < 0 || target >= current.questions.length) {
                return current;
            }

            const questions = [...current.questions];
            [questions[index], questions[target]] = [questions[target], questions[index]];
            return { ...current, questions };
        });

    const removeQuestion = (key: string) =>
        change((current) => ({
            ...current,
            questions: current.questions.filter((q) => q.key !== key),
        }));

    /** Checks everything the server would reject, with translated messages. */
    const validate = (): boolean => {
        const title = form.title.trim();
        const nextFieldErrors: ExerciseFieldErrors = {};

        if (title.length === 0 || title.length > exerciseRules.titleMaxLength) {
            nextFieldErrors.title = 'notificationExerciseTitleRequired';
        }

        if (form.subjectId === null) {
            nextFieldErrors.subjectId = 'notificationExerciseSubjectRequired';
        }

        if (form.grade === null) {
            nextFieldErrors.grade = 'notificationExerciseGradeRequired';
        }

        const nextQuestionErrors: Record<string, NotificationKey[]> = {};

        if (form.contentSource === 'questions') {
            for (const question of form.questions) {
                const problems = validateQuestion(question);
                if (problems.length > 0) {
                    nextQuestionErrors[question.key] = problems;
                }
            }
        }

        const nextGeneratorErrors =
            form.contentSource === 'generator' ? validateArithmeticSettings(form.generator) : [];

        setFieldErrors(nextFieldErrors);
        setQuestionErrors(nextQuestionErrors);
        setGeneratorErrors(nextGeneratorErrors);

        const tooMany =
            form.contentSource === 'questions' &&
            form.questions.length > exerciseRules.maxQuestions;

        const isValid =
            Object.keys(nextFieldErrors).length === 0 &&
            Object.keys(nextQuestionErrors).length === 0 &&
            nextGeneratorErrors.length === 0 &&
            !tooMany;

        setErrorKey(
            isValid
                ? null
                : tooMany
                  ? 'notificationTooManyQuestions'
                  : 'notificationExerciseHasErrors',
        );
        return isValid;
    };

    const toRequest = (): ISaveExerciseRequest => ({
        title: form.title.trim(),
        subjectId: form.subjectId ?? 0,
        grade: form.grade ?? 0,
        contentSource: form.contentSource,
        questions: form.contentSource === 'questions' ? form.questions.map(toQuestion) : [],
        generator: form.contentSource === 'generator' ? form.generator : null,
    });

    /** Server errors "questions[n]" go to the question card, everything else into the alert. */
    const showServerErrors = (problemErrors: Record<string, string>, fallback: NotificationKey) => {
        const nextQuestionErrors: Record<string, NotificationKey[]> = {};
        let other = false;

        for (const field of Object.keys(problemErrors)) {
            const match = questionIndexPattern.exec(field);
            const question = match ? form.questions[Number(match[1])] : undefined;

            if (question) {
                nextQuestionErrors[question.key] = ['notificationQuestionInvalid'];
            } else {
                other = true;
            }
        }

        setQuestionErrors(nextQuestionErrors);
        setErrorKey(
            other || Object.keys(nextQuestionErrors).length === 0
                ? fallback
                : 'notificationExerciseHasErrors',
        );
    };

    const saveDraft = async (): Promise<number | null> => {
        if (!validate()) {
            return null;
        }

        setIsSaving(true);
        const request = toRequest();
        const result = summary
            ? await exercisesApi.update.post({ body: { id: summary.id, exercise: request } })
            : await exercisesApi.create.post({ body: request });
        setIsSaving(false);

        if (!result.data) {
            showServerErrors(
                getFieldErrors(result.error?.problem),
                result.error?.messageKey ?? 'notificationUnexpectedError',
            );
            return null;
        }

        loadedId.current = result.data.id;
        setSummary(result.data);
        setIsDirty(false);
        return result.data.id;
    };

    const save = async (): Promise<number | null> => {
        const id = await saveDraft();
        if (id !== null) {
            setSuccessKey('notificationExerciseSaved');
        }

        return id;
    };

    const publish = async (): Promise<number | null> => {
        if (form.contentSource === 'questions' && form.questions.length === 0) {
            setErrorKey('notificationPublishNeedsQuestions');
            return null;
        }

        const id = isDirty || summary === null ? await saveDraft() : summary.id;
        if (id === null) {
            return null;
        }

        setIsSaving(true);
        const result = await exercisesApi.publish.post({ body: { id } });
        setIsSaving(false);

        if (!result.data) {
            showServerErrors(
                getFieldErrors(result.error?.problem),
                result.error?.messageKey ?? 'notificationUnexpectedError',
            );
            return null;
        }

        setSummary(result.data);
        setErrorKey(null);
        setSuccessKey('notificationExercisePublished');
        return id;
    };

    return {
        form,
        summary,
        subjects,
        isLoading,
        isSaving,
        isDirty,
        fieldErrors,
        questionErrors,
        generatorErrors,
        errorKey,
        successKey,
        update,
        addQuestion,
        updateQuestion,
        moveQuestion,
        removeQuestion,
        save,
        publish,
    };
};
