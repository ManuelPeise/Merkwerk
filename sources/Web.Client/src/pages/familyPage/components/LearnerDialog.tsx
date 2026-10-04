import React from 'react';
import {
    Alert,
    Box,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    IconButton,
    Stack,
    ToggleButton,
    ToggleButtonGroup,
    Typography,
    type DialogProps,
} from '@mui/material';
import AvatarImage from 'src/components/kids/AvatarImage';
import FormButton from 'src/components/input/FormButton';
import FormTextField from 'src/components/input/FormTextField';
import { avatarIds } from 'src/assets/avatars/avatars';
import { useTranslation } from 'src/hooks/useTranslation';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { learnersApi } from 'src/lib/api/learners/learnersApi';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import { testIdOf } from 'src/lib/testing/testIds';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** Mount it only while open (the form starts from the props). */
interface IProps {
    open: boolean;
    /** Edit this child; without it a new one is created. */
    learner?: ILearner;
    testId?: string;
    onClose: () => void;
    onSaved: () => void;
}

interface ILearnerForm {
    displayName: string;
    grade: number;
    avatarId: string;
}

const grades = [1, 2, 3, 4] as const;
const maxNameLength = 30;

const toForm = (learner?: ILearner): ILearnerForm => ({
    displayName: learner?.displayName ?? '',
    grade: learner?.grade ?? 1,
    avatarId: learner?.avatarId ?? avatarIds[0],
});

/** Create or edit a child: first name or nickname, grade, built-in picture – nothing else (privacy). */
const LearnerDialog: React.FC<IProps> = (props) => {
    const { open, learner, testId, onClose, onSaved } = props;
    const { getResource } = useTranslation();

    const [form, setForm] = React.useState<ILearnerForm>(() => toForm(learner));
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSaving, setIsSaving] = React.useState(false);

    const name = form.displayName.trim();
    const isValid = name.length > 0 && name.length <= maxNameLength;

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setErrorKey(null);

        const body = { displayName: name, grade: form.grade, avatarId: form.avatarId };
        const result = learner
            ? await learnersApi.update.post({ body: { id: learner.id, ...body } })
            : await learnersApi.create.post({ body });

        setIsSaving(false);

        if (result.error) {
            const errors = getFieldErrors(result.error.problem);
            setFieldErrors(errors);

            if (result.error.status === 409) {
                setErrorKey('notificationChildLimitReached');
            } else if (Object.keys(errors).length === 0) {
                setErrorKey(result.error.messageKey);
            }

            return;
        }

        onSaved();
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="sm"
            fullWidth
            slotProps={{
                paper: {
                    'data-testid': testId,
                } as NonNullable<DialogProps['slotProps']>['paper'],
            }}
        >
            <form onSubmit={handleSubmit} noValidate>
                <DialogTitle>
                    {getResource(learner ? 'captionEditChild' : 'captionAddChild')}
                </DialogTitle>
                <DialogContent>
                    <Stack spacing={3} sx={{ pt: 1 }}>
                        {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                        <FormTextField
                            label={getResource('labelChildName')}
                            name="displayName"
                            required
                            value={form.displayName}
                            errorText={fieldErrors.displayName}
                            helperText={getResource('captionChildNameHint')}
                            testId={testIdOf(testId, 'name')}
                            onChange={(displayName) => setForm({ ...form, displayName })}
                        />
                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelGrade')}
                            </Typography>
                            <ToggleButtonGroup
                                exclusive
                                value={form.grade}
                                aria-label={getResource('labelGrade')}
                                onChange={(_, grade: number | null) => {
                                    if (grade !== null) {
                                        setForm({ ...form, grade });
                                    }
                                }}
                            >
                                {grades.map((grade) => (
                                    <ToggleButton
                                        key={grade}
                                        value={grade}
                                        sx={{ minWidth: 64, minHeight: 56 }}
                                    >
                                        {getResource('captionGrade', { grade })}
                                    </ToggleButton>
                                ))}
                            </ToggleButtonGroup>
                        </Box>
                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelAvatar')}
                            </Typography>
                            <Box
                                role="radiogroup"
                                aria-label={getResource('labelAvatar')}
                                sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}
                            >
                                {avatarIds.map((avatarId, index) => {
                                    const isSelected = form.avatarId === avatarId;

                                    return (
                                        <IconButton
                                            key={avatarId}
                                            role="radio"
                                            aria-checked={isSelected}
                                            aria-label={getResource('labelAvatarOption', {
                                                value: index + 1,
                                            })}
                                            onClick={() => setForm({ ...form, avatarId })}
                                            sx={{
                                                p: 0.5,
                                                border: '3px solid',
                                                borderColor: isSelected
                                                    ? 'primary.main'
                                                    : 'transparent',
                                            }}
                                        >
                                            <AvatarImage
                                                avatarId={avatarId}
                                                name={name || '?'}
                                                size={56}
                                            />
                                        </IconButton>
                                    );
                                })}
                            </Box>
                        </Box>
                    </Stack>
                </DialogContent>
                <DialogActions sx={{ p: 2, gap: 1 }}>
                    <FormButton
                        label={getResource('labelCancel')}
                        intent="cancel"
                        testId={testIdOf(testId, 'cancel')}
                        onClick={onClose}
                    />
                    <FormButton
                        label={getResource('labelSave')}
                        type="submit"
                        disabled={!isValid || isSaving}
                        testId={testIdOf(testId, 'save')}
                    />
                </DialogActions>
            </form>
        </Dialog>
    );
};

export default LearnerDialog;
