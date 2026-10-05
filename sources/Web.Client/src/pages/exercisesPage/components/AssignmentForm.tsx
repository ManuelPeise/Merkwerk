import React from 'react';
import { Box, Stack, Typography } from '@mui/material';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormDateField from 'src/components/input/FormDateField';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IAssignmentForm } from 'src/hooks/useExerciseAssignments';
import type { IAssignment } from 'src/lib/api/assignments/assignmentsTypes';
import type { IGroup } from 'src/lib/api/groups/groupsTypes';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import { todayValue } from 'src/lib/assignments/dueDates';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import GeneratorForm from 'src/pages/exerciseEditorPage/components/GeneratorForm';

interface IProps {
    form: IAssignmentForm;
    learners: ILearner[];
    groups: IGroup[];
    /** To mark children and groups that already have the exercise (assigning again changes their options). */
    assignments: IAssignment[];
    isGenerator: boolean;
    generatorProblems: NotificationKey[];
    dueDateError: NotificationKey | null;
    disabled: boolean;
    uiTestId: string;
    onChange: (change: Partial<IAssignmentForm>) => void;
    onToggleLearner: (learnerId: number, checked: boolean) => void;
    onToggleGroup: (groupId: number, checked: boolean) => void;
}

/** Children and groups to assign to, due date, allowed aids and - for generators - own settings (LP-114). */
const AssignmentForm: React.FC<IProps> = (props) => {
    const {
        form,
        learners,
        groups,
        assignments,
        isGenerator,
        generatorProblems,
        dueDateError,
        disabled,
        uiTestId,
        onChange,
        onToggleLearner,
        onToggleGroup,
    } = props;
    const { getResource } = useTranslation();

    const caption = (text: string) => (
        <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
            {text}
        </Typography>
    );

    const withHint = (name: string, isAssigned: boolean) =>
        isAssigned ? getResource('labelAlreadyAssigned', { name }) : name;

    return (
        <Stack spacing={3} data-testid={uiTestId}>
            <Box>
                {caption(getResource('captionAssignChildren'))}
                {learners.length === 0 ? (
                    <Typography color="text.secondary">
                        {getResource('captionAssignNoChildren')}
                    </Typography>
                ) : (
                    <Stack spacing={0.5}>
                        {learners.map((learner) => (
                            <FormCheckbox
                                key={learner.id}
                                label={withHint(
                                    learner.displayName,
                                    assignments.some((a) => a.learnerId === learner.id),
                                )}
                                checked={form.learnerIds.includes(learner.id)}
                                disabled={disabled}
                                uiTestId={uiTestIdOf(uiTestId, `learner-${learner.id}`)}
                                onChange={(checked) => onToggleLearner(learner.id, checked)}
                            />
                        ))}
                    </Stack>
                )}
            </Box>

            {groups.length > 0 && (
                <Box>
                    {caption(getResource('captionAssignGroups'))}
                    <Typography color="text.secondary" sx={{ mb: 1 }}>
                        {getResource('captionAssignGroupsHint')}
                    </Typography>
                    <Stack spacing={0.5}>
                        {groups.map((group) => (
                            <FormCheckbox
                                key={group.id}
                                label={withHint(
                                    getResource('captionAssignmentGroup', { name: group.name }),
                                    assignments.some((a) => a.groupId === group.id),
                                )}
                                checked={form.groupIds.includes(group.id)}
                                disabled={disabled}
                                uiTestId={uiTestIdOf(uiTestId, `group-${group.id}`)}
                                onChange={(checked) => onToggleGroup(group.id, checked)}
                            />
                        ))}
                    </Stack>
                </Box>
            )}

            <FormDateField
                label={getResource('labelDueDate')}
                name="dueDate"
                value={form.dueDate}
                min={todayValue()}
                disabled={disabled}
                errorText={dueDateError ? getResource(dueDateError) : undefined}
                helperText={getResource('captionDueDateHint')}
                uiTestId={uiTestIdOf(uiTestId, 'due-date')}
                onChange={(dueDate) => onChange({ dueDate })}
            />

            <Box>
                {caption(getResource('captionLearningAids'))}
                <Stack spacing={0.5}>
                    <FormCheckbox
                        label={getResource('labelAllowDotArray')}
                        checked={form.allowDotArray}
                        disabled={disabled}
                        uiTestId={uiTestIdOf(uiTestId, 'allow-dot-array')}
                        onChange={(allowDotArray) => onChange({ allowDotArray })}
                    />
                    <FormCheckbox
                        label={getResource('labelAllowTimesTableMatrix')}
                        checked={form.allowTimesTableMatrix}
                        disabled={disabled}
                        uiTestId={uiTestIdOf(uiTestId, 'allow-times-table')}
                        onChange={(allowTimesTableMatrix) => onChange({ allowTimesTableMatrix })}
                    />
                </Stack>
            </Box>

            {isGenerator && (
                <Stack spacing={2}>
                    <FormCheckbox
                        label={getResource('labelOwnGeneratorSettings')}
                        checked={form.useOwnSettings}
                        disabled={disabled}
                        uiTestId={uiTestIdOf(uiTestId, 'own-settings')}
                        onChange={(useOwnSettings) => onChange({ useOwnSettings })}
                    />
                    {form.useOwnSettings && (
                        <GeneratorForm
                            settings={form.generator}
                            problems={generatorProblems}
                            uiTestId={uiTestIdOf(uiTestId, 'generator')}
                            onChange={(generator) => onChange({ generator })}
                        />
                    )}
                </Stack>
            )}
        </Stack>
    );
};

export default AssignmentForm;
