import React from 'react';
import { Box, Stack, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material';
import FormSelectField from 'src/components/input/FormSelectField';
import FormTextField from 'src/components/input/FormTextField';
import type {
    EditorContentSource,
    ExerciseFieldErrors,
    IExerciseForm,
} from 'src/hooks/useExerciseEditor';
import { useTranslation } from 'src/hooks/useTranslation';
import type { ISubject } from 'src/lib/api/subjects/subjectsTypes';
import { exerciseRules } from 'src/lib/exercises/exerciseRules';
import { uiTestIdOf } from 'src/lib/testing/uiTestId';

interface IProps {
    form: IExerciseForm;
    subjects: ISubject[];
    fieldErrors: ExerciseFieldErrors;
    /** Switching to the generator only while there are no questions (they would be dropped). */
    canChangeSource: boolean;
    uiTestId: string;
    onChange: (change: Partial<IExerciseForm>) => void;
}

const grades = Array.from(
    { length: exerciseRules.maxGrade - exerciseRules.minGrade + 1 },
    (_, index) => exerciseRules.minGrade + index,
);

/** Title, subject, grade and where the tasks come from. */
const ExerciseDetailsForm: React.FC<IProps> = (props) => {
    const { form, subjects, fieldErrors, canChangeSource, uiTestId, onChange } = props;
    const { getResource } = useTranslation();

    const errorOf = (field: keyof ExerciseFieldErrors) => {
        const key = fieldErrors[field];
        return key ? getResource(key) : undefined;
    };

    return (
        <Stack spacing={3}>
            <FormTextField
                label={getResource('labelExerciseTitle')}
                name="title"
                required
                value={form.title}
                errorText={errorOf('title')}
                helperText={getResource('captionExerciseTitleHint')}
                uiTestId={uiTestIdOf(uiTestId, 'title')}
                onChange={(title) => onChange({ title })}
            />

            <Box
                sx={{ display: 'grid', gap: 3, gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' } }}
            >
                <FormSelectField
                    label={getResource('labelExerciseSubject')}
                    required
                    value={form.subjectId === null ? '' : String(form.subjectId)}
                    placeholder={getResource('labelChooseSubject')}
                    options={subjects.map((subject) => ({
                        value: String(subject.id),
                        label: subject.name,
                    }))}
                    errorText={errorOf('subjectId')}
                    uiTestId={uiTestIdOf(uiTestId, 'subject')}
                    onChange={(value) =>
                        onChange({ subjectId: value === '' ? null : Number(value) })
                    }
                />
                <FormSelectField
                    label={getResource('labelGrade')}
                    required
                    value={form.grade === null ? '' : String(form.grade)}
                    placeholder={getResource('labelChooseGrade')}
                    options={grades.map((grade) => ({
                        value: String(grade),
                        label: getResource('captionGrade', { grade }),
                    }))}
                    errorText={errorOf('grade')}
                    uiTestId={uiTestIdOf(uiTestId, 'grade')}
                    onChange={(value) => onChange({ grade: value === '' ? null : Number(value) })}
                />
            </Box>

            <Box>
                <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                    {getResource('labelExerciseContentSource')}
                </Typography>
                <ToggleButtonGroup
                    exclusive
                    value={form.contentSource}
                    aria-label={getResource('labelExerciseContentSource')}
                    sx={{ flexWrap: 'wrap' }}
                    data-testid={uiTestIdOf(uiTestId, 'source')}
                    onChange={(_, contentSource: EditorContentSource | null) => {
                        if (contentSource !== null) {
                            onChange({ contentSource });
                        }
                    }}
                >
                    <ToggleButton value="questions" sx={{ minHeight: 56 }}>
                        {getResource('labelContentSourceQuestions')}
                    </ToggleButton>
                    <ToggleButton
                        value="generator"
                        disabled={!canChangeSource}
                        sx={{ minHeight: 56 }}
                    >
                        {getResource('labelContentSourceGenerator')}
                    </ToggleButton>
                </ToggleButtonGroup>
                {!canChangeSource && (
                    <Typography color="text.secondary" sx={{ mt: 1 }}>
                        {getResource('captionContentSourceLocked')}
                    </Typography>
                )}
            </Box>
        </Stack>
    );
};

export default ExerciseDetailsForm;
