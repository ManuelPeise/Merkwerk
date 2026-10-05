import React from 'react';
import { defaultArithmeticSettings } from 'src/hooks/useExerciseEditor';
import { assignmentsApi } from 'src/lib/api/assignments/assignmentsApi';
import type { IAssignment } from 'src/lib/api/assignments/assignmentsTypes';
import { exercisesApi } from 'src/lib/api/exercises/exercisesApi';
import type { IArithmeticSettings, IExerciseSummary } from 'src/lib/api/exercises/exercisesTypes';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { groupsApi } from 'src/lib/api/groups/groupsApi';
import type { IGroup } from 'src/lib/api/groups/groupsTypes';
import { learnersApi } from 'src/lib/api/learners/learnersApi';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import { validateArithmeticSettings } from 'src/lib/exercises/exerciseRules';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** What the adult picks before pressing "Assign". */
export interface IAssignmentForm {
    learnerIds: number[];
    groupIds: number[];
    /** "yyyy-MM-dd" or empty. */
    dueDate: string;
    allowDotArray: boolean;
    allowTimesTableMatrix: boolean;
    /** Generator exercises only: use generator instead of the exercise's own settings. */
    useOwnSettings: boolean;
    generator: IArithmeticSettings;
}

interface IExerciseAssignments {
    /** Null while loading. */
    assignments: IAssignment[] | null;
    learners: ILearner[];
    groups: IGroup[];
    /** The exercise's own generator settings (generator exercises), otherwise null. */
    exerciseGenerator: IArithmeticSettings | null;
    form: IAssignmentForm;
    generatorProblems: NotificationKey[];
    canAssign: boolean;
    isBusy: boolean;
    errorKey: NotificationKey | null;
    successKey: NotificationKey | null;
    dueDateError: NotificationKey | null;
    updateForm: (change: Partial<IAssignmentForm>) => void;
    toggleLearner: (learnerId: number, checked: boolean) => void;
    toggleGroup: (groupId: number, checked: boolean) => void;
    /** Puts one existing assignment into the form, so assigning again changes it. */
    edit: (assignment: IAssignment) => void;
    assign: () => Promise<void>;
    revoke: (assignment: IAssignment) => Promise<void>;
}

const createForm = (generator: IArithmeticSettings | null): IAssignmentForm => ({
    learnerIds: [],
    groupIds: [],
    dueDate: '',
    allowDotArray: false,
    allowTimesTableMatrix: false,
    useOwnSettings: false,
    generator: generator ?? defaultArithmeticSettings,
});

const toggle = (ids: number[], id: number, checked: boolean): number[] =>
    checked ? [...ids.filter((value) => value !== id), id] : ids.filter((value) => value !== id);

/** Assignments of one exercise and the form to add or change them (LP-114). */
export const useExerciseAssignments = (exercise: IExerciseSummary): IExerciseAssignments => {
    const [assignments, setAssignments] = React.useState<IAssignment[] | null>(null);
    const [learners, setLearners] = React.useState<ILearner[]>([]);
    const [groups, setGroups] = React.useState<IGroup[]>([]);
    const [exerciseGenerator, setExerciseGenerator] = React.useState<IArithmeticSettings | null>(
        null,
    );
    const [form, setForm] = React.useState<IAssignmentForm>(() => createForm(null));
    const [isBusy, setIsBusy] = React.useState(false);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [successKey, setSuccessKey] = React.useState<NotificationKey | null>(null);
    const [dueDateError, setDueDateError] = React.useState<NotificationKey | null>(null);

    const isGenerator = exercise.contentSource === 'generator';

    React.useEffect(() => {
        const controller = new AbortController();
        const signal = controller.signal;

        void Promise.all([
            assignmentsApi.list.get({ params: { exerciseId: exercise.id }, signal }),
            learnersApi.list.get({ signal }),
            groupsApi.list.get({ signal }),
            isGenerator ? exercisesApi.get.get({ params: { id: exercise.id }, signal }) : null,
        ]).then(([assignmentResult, learnerResult, groupResult, detailResult]) => {
            if (assignmentResult.error?.kind === 'canceled') {
                return;
            }

            const generator = detailResult?.data?.generator ?? null;

            setAssignments(assignmentResult.data ?? []);
            setLearners(learnerResult.data ?? []);
            setGroups(groupResult.data ?? []);
            setExerciseGenerator(generator);
            setForm(createForm(generator));
            setErrorKey(
                assignmentResult.error?.messageKey ??
                    learnerResult.error?.messageKey ??
                    groupResult.error?.messageKey ??
                    null,
            );
        });

        return () => controller.abort();
    }, [exercise.id, isGenerator]);

    const generatorProblems =
        isGenerator && form.useOwnSettings ? validateArithmeticSettings(form.generator) : [];
    const canAssign =
        form.learnerIds.length + form.groupIds.length > 0 &&
        generatorProblems.length === 0 &&
        !isBusy;

    const updateForm = (change: Partial<IAssignmentForm>) => {
        setSuccessKey(null);
        setForm((current) => ({ ...current, ...change }));
    };

    const toggleLearner = (learnerId: number, checked: boolean) =>
        updateForm({ learnerIds: toggle(form.learnerIds, learnerId, checked) });

    const toggleGroup = (groupId: number, checked: boolean) =>
        updateForm({ groupIds: toggle(form.groupIds, groupId, checked) });

    const edit = (assignment: IAssignment) => {
        setErrorKey(null);
        setSuccessKey(null);
        setDueDateError(null);
        setForm({
            learnerIds: assignment.learnerId === null ? [] : [assignment.learnerId],
            groupIds: assignment.groupId === null ? [] : [assignment.groupId],
            dueDate: assignment.dueDate ?? '',
            allowDotArray: assignment.allowDotArray,
            allowTimesTableMatrix: assignment.allowTimesTableMatrix,
            useOwnSettings: assignment.generator !== null,
            generator: assignment.generator ?? exerciseGenerator ?? defaultArithmeticSettings,
        });
    };

    const reload = async () => {
        const result = await assignmentsApi.list.get({ params: { exerciseId: exercise.id } });

        if (result.data) {
            setAssignments(result.data);
        }
    };

    const assign = async () => {
        if (!canAssign) {
            return;
        }

        setIsBusy(true);
        setErrorKey(null);
        setSuccessKey(null);
        setDueDateError(null);

        const result = await assignmentsApi.assign.post({
            body: {
                exerciseId: exercise.id,
                learnerIds: form.learnerIds,
                groupIds: form.groupIds,
                dueDate: form.dueDate === '' ? null : form.dueDate,
                allowDotArray: form.allowDotArray,
                allowTimesTableMatrix: form.allowTimesTableMatrix,
                generator: isGenerator && form.useOwnSettings ? form.generator : null,
            },
        });

        if (result.error) {
            setIsBusy(false);
            const fields = getFieldErrors(result.error.problem);

            if (fields.dueDate) {
                setDueDateError('notificationDueDateInPast');
            } else if (fields.exerciseId) {
                setErrorKey('notificationExerciseNotAssignable');
            } else if (fields.learnerIds || fields.groupIds) {
                // A child or group was deleted meanwhile - the lists are outdated.
                setErrorKey('notificationAssignTargetsChanged');
            } else if (Object.keys(fields).length > 0) {
                setErrorKey('notificationAssignmentInvalid');
            } else {
                setErrorKey(result.error.messageKey);
            }

            return;
        }

        await reload();
        setIsBusy(false);
        setForm(createForm(exerciseGenerator));
        setSuccessKey('notificationAssignmentSaved');
    };

    const revoke = async (assignment: IAssignment) => {
        setIsBusy(true);
        setErrorKey(null);
        setSuccessKey(null);

        const result = await assignmentsApi.revoke.post({ body: { id: assignment.id } });

        if (result.error && result.error.status !== 404) {
            setIsBusy(false);
            setErrorKey(result.error.messageKey);
            return;
        }

        // 404: someone else took it back already - the list catches up either way.
        await reload();
        setIsBusy(false);
        setSuccessKey('notificationAssignmentRevoked');
    };

    return {
        assignments,
        learners,
        groups,
        exerciseGenerator,
        form,
        generatorProblems,
        canAssign,
        isBusy,
        errorKey,
        successKey,
        dueDateError,
        updateForm,
        toggleLearner,
        toggleGroup,
        edit,
        assign,
        revoke,
    };
};
