import React from 'react';
import { Alert, Box, Button, Card, CardContent, Stack, Typography } from '@mui/material';
import { Link, useNavigate, useParams } from 'react-router-dom';
import ExerciseStateChip from 'src/components/exercises/ExerciseStateChip';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { BackIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import { useExerciseEditor } from 'src/hooks/useExerciseEditor';
import { useTranslation } from 'src/hooks/useTranslation';
import { uiTestId } from 'src/lib/testing/uiTestId';
import { routes, toExerciseEditor } from 'src/navigation/routes';
import ExerciseDetailsForm from 'src/pages/exerciseEditorPage/components/ExerciseDetailsForm';
import GeneratorForm from 'src/pages/exerciseEditorPage/components/GeneratorForm';
import GeneratorPreview from 'src/pages/exerciseEditorPage/components/GeneratorPreview';
import QuestionList from 'src/pages/exerciseEditorPage/components/QuestionList';

/** "new" or a positive id; anything else counts as unknown. */
const parseId = (id: string | undefined): number | null | undefined => {
    if (id === 'new') {
        return null;
    }

    const value = Number(id);
    return Number.isInteger(value) && value > 0 ? value : undefined;
};

/** /admin/exercises/:id - create or edit an exercise with questions or a generator (LP-112). */
const ExerciseEditorPage: React.FC = () => {
    const { id } = useParams();
    const { getResource } = useTranslation();
    const navigate = useNavigate();

    const exerciseId = parseId(id);
    const editor = useExerciseEditor(exerciseId ?? null);
    const { form, summary, subjects, isLoading, isSaving, isDirty, errorKey, successKey } = editor;

    if (exerciseId === undefined) {
        return <Alert severity="error">{getResource('notificationNotFound')}</Alert>;
    }

    const afterSave = (savedId: number | null) => {
        // A new exercise gets its own URL, so reloading the page keeps it.
        if (savedId !== null && exerciseId === null) {
            navigate(toExerciseEditor(savedId), { replace: true });
        }
    };

    const handleSave = async () => afterSave(await editor.save());

    const handlePublish = async () => afterSave(await editor.publish());

    if (isLoading || subjects === null) {
        return <LoadingIndicator uiTestId={uiTestId('loading-exercise-editor-page')} />;
    }

    return (
        <Stack spacing={3} data-testid={uiTestId('exercise-editor-page')}>
            <Box>
                <Button component={Link} to={routes.exercises} startIcon={<BackIcon />}>
                    {getResource('labelBackToExercises')}
                </Button>
            </Box>

            <Box sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', gap: 2 }}>
                <Typography variant="h1">
                    {summary ? summary.title : getResource('captionNewExercise')}
                </Typography>
                {summary && (
                    <ExerciseStateChip
                        exercise={{
                            ...summary,
                            hasUnpublishedChanges: summary.hasUnpublishedChanges || isDirty,
                        }}
                        uiTestId={uiTestId('exercise-editor-state')}
                    />
                )}
            </Box>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
            {successKey && <Alert severity="success">{getResource(successKey)}</Alert>}

            <Card>
                <CardContent>
                    <ExerciseDetailsForm
                        form={form}
                        subjects={subjects}
                        fieldErrors={editor.fieldErrors}
                        canChangeSource={
                            form.contentSource === 'generator' || form.questions.length === 0
                        }
                        uiTestId={uiTestId('exercise-editor-details')}
                        onChange={editor.update}
                    />
                </CardContent>
            </Card>

            {form.contentSource === 'questions' ? (
                <QuestionList
                    questions={form.questions}
                    questionErrors={editor.questionErrors}
                    uiTestId={uiTestId('exercise-editor-questions')}
                    onAdd={editor.addQuestion}
                    onChange={editor.updateQuestion}
                    onMove={editor.moveQuestion}
                    onRemove={editor.removeQuestion}
                />
            ) : (
                <Card>
                    <CardContent>
                        <Stack spacing={3}>
                            <GeneratorForm
                                settings={form.generator}
                                problems={editor.generatorErrors}
                                uiTestId={uiTestId('exercise-editor-generator')}
                                onChange={(generator) => editor.update({ generator })}
                            />
                            <GeneratorPreview
                                settings={form.generator}
                                uiTestId={uiTestId('exercise-editor-generator-preview')}
                            />
                        </Stack>
                    </CardContent>
                </Card>
            )}

            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'flex-end', gap: 1 }}>
                <FormButton
                    label={getResource('labelSaveDraft')}
                    intent="cancel"
                    disabled={isSaving}
                    uiTestId={uiTestId('exercise-editor-save-button')}
                    onClick={() => void handleSave()}
                />
                <FormButton
                    label={getResource('labelPublishExercise')}
                    disabled={
                        isSaving ||
                        (summary !== null &&
                            summary.state !== 'draft' &&
                            !summary.hasUnpublishedChanges &&
                            !isDirty)
                    }
                    uiTestId={uiTestId('exercise-editor-publish-button')}
                    onClick={() => void handlePublish()}
                />
            </Box>
        </Stack>
    );
};

export default ExerciseEditorPage;
