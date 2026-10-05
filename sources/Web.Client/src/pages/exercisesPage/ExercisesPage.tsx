import React from 'react';
import {
    Alert,
    Box,
    Card,
    IconButton,
    List,
    ListItem,
    ListItemButton,
    ListItemText,
    Stack,
    Typography,
} from '@mui/material';
import { Link, useNavigate } from 'react-router-dom';
import ExerciseStateChip from 'src/components/exercises/ExerciseStateChip';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { ArchiveIcon, AssignIcon, RestoreIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import FormCheckbox from 'src/components/input/FormCheckbox';
import SubjectBadge from 'src/components/subjects/SubjectBadge';
import { useTranslation } from 'src/hooks/useTranslation';
import { exercisesApi } from 'src/lib/api/exercises/exercisesApi';
import type { IExerciseSummary } from 'src/lib/api/exercises/exercisesTypes';
import { subjectsApi } from 'src/lib/api/subjects/subjectsApi';
import type { ISubject } from 'src/lib/api/subjects/subjectsTypes';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { toExerciseEditor } from 'src/navigation/routes';
import AssignmentsDialog from 'src/pages/exercisesPage/components/AssignmentsDialog';

/**
 * /admin/exercises - the family's exercises (LP-112). Every adult creates, edits, publishes and archives them, and assigns
 * published ones to children and groups (LP-114).
 */
const ExercisesPage: React.FC = () => {
    const { getResource } = useTranslation();
    const navigate = useNavigate();

    const [exercises, setExercises] = React.useState<IExerciseSummary[] | null>(null);
    const [subjects, setSubjects] = React.useState<ISubject[]>([]);
    const [showArchived, setShowArchived] = React.useState(false);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [busyId, setBusyId] = React.useState<number | null>(null);
    const [assigning, setAssigning] = React.useState<IExerciseSummary | null>(null);

    React.useEffect(() => {
        const controller = new AbortController();

        void Promise.all([
            exercisesApi.list.get({ signal: controller.signal }),
            subjectsApi.list.get({ signal: controller.signal }),
        ]).then(([exerciseResult, subjectResult]) => {
            if (exerciseResult.error?.kind === 'canceled') {
                return;
            }

            setExercises(exerciseResult.data ?? []);
            setSubjects(subjectResult.data ?? []);
            setErrorKey(exerciseResult.error ? exerciseResult.error.messageKey : null);
        });

        return () => controller.abort();
    }, []);

    const handleArchive = async (exercise: IExerciseSummary) => {
        setBusyId(exercise.id);
        const result = await exercisesApi.archive.post({
            body: { id: exercise.id, archived: exercise.state !== 'archived' },
        });
        setBusyId(null);

        if (!result.data) {
            setErrorKey(result.error?.messageKey ?? 'notificationUnexpectedError');
            return;
        }

        const changed = result.data;
        setErrorKey(null);
        setExercises((current) => current?.map((e) => (e.id === changed.id ? changed : e)) ?? null);
    };

    const visible = (exercises ?? []).filter((e) => showArchived || e.state !== 'archived');
    const subjectOf = (id: number) => subjects.find((subject) => subject.id === id);

    return (
        <Stack spacing={4} data-testid={uiTestId('exercises-page')}>
            <Box
                sx={{
                    display: 'flex',
                    flexWrap: 'wrap',
                    alignItems: 'flex-start',
                    justifyContent: 'space-between',
                    gap: 2,
                }}
            >
                <Stack spacing={1}>
                    <Typography variant="h1">{getResource('captionExercises')}</Typography>
                    <Typography color="text.secondary">
                        {getResource('captionExercisesDescription')}
                    </Typography>
                </Stack>
                <FormButton
                    label={getResource('labelAddExercise')}
                    uiTestId={uiTestId('exercises-add-button')}
                    onClick={() => navigate(toExerciseEditor('new'))}
                />
            </Box>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            <FormCheckbox
                label={getResource('labelShowArchivedExercises')}
                checked={showArchived}
                uiTestId={uiTestId('exercises-show-archived')}
                onChange={setShowArchived}
            />

            {exercises === null ? (
                <LoadingIndicator uiTestId={uiTestId('loading-exercises-page')} />
            ) : visible.length === 0 ? (
                <Typography color="text.secondary">{getResource('captionNoExercises')}</Typography>
            ) : (
                <Card>
                    <List data-testid={uiTestId('exercises-list')}>
                        {visible.map((exercise) => {
                            const subject = subjectOf(exercise.subjectId);
                            const isArchived = exercise.state === 'archived';
                            const canAssign = exercise.state === 'published';

                            return (
                                <ListItem
                                    key={exercise.id}
                                    disablePadding
                                    data-testid={uiTestIdOf(
                                        uiTestId('exercises-item'),
                                        String(exercise.id),
                                    )}
                                    secondaryAction={
                                        <Box sx={{ display: 'flex', gap: 0.5 }}>
                                            {canAssign && (
                                                <IconButton
                                                    aria-label={getResource('labelAssignExercise', {
                                                        title: exercise.title,
                                                    })}
                                                    data-testid={uiTestIdOf(
                                                        uiTestId('exercises-assign'),
                                                        String(exercise.id),
                                                    )}
                                                    onClick={() => setAssigning(exercise)}
                                                >
                                                    <AssignIcon />
                                                </IconButton>
                                            )}
                                            <IconButton
                                                disabled={busyId === exercise.id}
                                                aria-label={getResource(
                                                    isArchived
                                                        ? 'labelRestoreExercise'
                                                        : 'labelArchiveExercise',
                                                    { title: exercise.title },
                                                )}
                                                onClick={() => void handleArchive(exercise)}
                                            >
                                                {isArchived ? <RestoreIcon /> : <ArchiveIcon />}
                                            </IconButton>
                                        </Box>
                                    }
                                >
                                    <ListItemButton
                                        component={Link}
                                        to={toExerciseEditor(exercise.id)}
                                        sx={{ pr: canAssign ? 14 : 8 }}
                                    >
                                        <ListItemText
                                            disableTypography
                                            primary={
                                                <Typography sx={{ fontWeight: 700 }}>
                                                    {exercise.title}
                                                </Typography>
                                            }
                                            secondary={
                                                <Box
                                                    sx={{
                                                        display: 'flex',
                                                        flexWrap: 'wrap',
                                                        alignItems: 'center',
                                                        gap: 1.5,
                                                        mt: 1,
                                                    }}
                                                >
                                                    {subject && (
                                                        <SubjectBadge
                                                            subject={subject}
                                                            uiTestId={uiTestIdOf(
                                                                uiTestId('exercises-subject'),
                                                                String(exercise.id),
                                                            )}
                                                        />
                                                    )}
                                                    <Typography color="text.secondary">
                                                        {getResource('captionGrade', {
                                                            grade: exercise.grade,
                                                        })}
                                                    </Typography>
                                                    <Typography color="text.secondary">
                                                        {getResource('captionExerciseTaskCount', {
                                                            count: exercise.questionCount,
                                                        })}
                                                    </Typography>
                                                    <ExerciseStateChip
                                                        exercise={exercise}
                                                        uiTestId={uiTestIdOf(
                                                            uiTestId('exercises-state'),
                                                            String(exercise.id),
                                                        )}
                                                    />
                                                </Box>
                                            }
                                        />
                                    </ListItemButton>
                                </ListItem>
                            );
                        })}
                    </List>
                </Card>
            )}

            {assigning && (
                <AssignmentsDialog
                    open
                    exercise={assigning}
                    uiTestId={uiTestId('exercises-assign-dialog')}
                    onClose={() => setAssigning(null)}
                />
            )}
        </Stack>
    );
};

export default ExercisesPage;
