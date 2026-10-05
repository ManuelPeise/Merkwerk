import React from 'react';
import {
    Alert,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    Stack,
    Typography,
    type DialogProps,
} from '@mui/material';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import FormButton from 'src/components/input/FormButton';
import { useExerciseAssignments } from 'src/hooks/useExerciseAssignments';
import { useTranslation } from 'src/hooks/useTranslation';
import type { IExerciseSummary } from 'src/lib/api/exercises/exercisesTypes';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';
import AssignmentForm from 'src/pages/exercisesPage/components/AssignmentForm';
import AssignmentList from 'src/pages/exercisesPage/components/AssignmentList';

/** Mount it only while open (it loads when it appears). */
interface IProps {
    open: boolean;
    exercise: IExerciseSummary;
    uiTestId: string;
    onClose: () => void;
}

/** Who has the exercise, and assign it to more children or groups (LP-114). */
const AssignmentsDialog: React.FC<IProps> = (props) => {
    const { open, exercise, uiTestId, onClose } = props;
    const { getResource } = useTranslation();
    const state = useExerciseAssignments(exercise);

    const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        void state.assign();
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="sm"
            fullWidth
            scroll="body"
            slotProps={{
                paper: {
                    'data-testid': uiTestId,
                } as NonNullable<DialogProps['slotProps']>['paper'],
            }}
        >
            <form onSubmit={handleSubmit} noValidate>
                <DialogTitle>
                    {getResource('captionAssignExercise', { title: exercise.title })}
                </DialogTitle>
                <DialogContent>
                    {state.assignments === null ? (
                        <LoadingIndicator uiTestId={uiTestIdOf(uiTestId, 'loading')} />
                    ) : (
                        <Stack spacing={3} sx={{ pt: 1 }}>
                            {state.errorKey && (
                                <Alert severity="error">{getResource(state.errorKey)}</Alert>
                            )}
                            {state.successKey && (
                                <Alert severity="success">{getResource(state.successKey)}</Alert>
                            )}

                            <Stack spacing={1}>
                                <Typography
                                    variant="h2"
                                    component="h3"
                                    sx={{ fontSize: '1.25rem' }}
                                >
                                    {getResource('captionAssignedTo')}
                                </Typography>
                                <AssignmentList
                                    assignments={state.assignments}
                                    learners={state.learners}
                                    groups={state.groups}
                                    disabled={state.isBusy}
                                    uiTestId={uiTestIdOf(uiTestId, 'list')}
                                    onEdit={state.edit}
                                    onRevoke={(assignment) => void state.revoke(assignment)}
                                />
                            </Stack>

                            <Divider />

                            <Stack spacing={2}>
                                <Typography
                                    variant="h2"
                                    component="h3"
                                    sx={{ fontSize: '1.25rem' }}
                                >
                                    {getResource('captionAssignTo')}
                                </Typography>
                                <AssignmentForm
                                    form={state.form}
                                    learners={state.learners}
                                    groups={state.groups}
                                    assignments={state.assignments}
                                    isGenerator={exercise.contentSource === 'generator'}
                                    generatorProblems={state.generatorProblems}
                                    dueDateError={state.dueDateError}
                                    disabled={state.isBusy}
                                    uiTestId={uiTestIdOf(uiTestId, 'form')}
                                    onChange={state.updateForm}
                                    onToggleLearner={state.toggleLearner}
                                    onToggleGroup={state.toggleGroup}
                                />
                            </Stack>
                        </Stack>
                    )}
                </DialogContent>
                <DialogActions sx={{ p: 2, gap: 1 }}>
                    <FormButton
                        label={getResource('labelClose')}
                        intent="cancel"
                        uiTestId={uiTestIdOf(uiTestId, 'close')}
                        onClick={onClose}
                    />
                    <FormButton
                        label={getResource('labelAssign')}
                        type="submit"
                        disabled={!state.canAssign}
                        uiTestId={uiTestIdOf(uiTestId, 'assign')}
                    />
                </DialogActions>
            </form>
        </Dialog>
    );
};

export default AssignmentsDialog;
