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
} from '@mui/material';
import { CheckIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import FormTextField from 'src/components/input/FormTextField';
import SubjectBadge from 'src/components/subjects/SubjectBadge';
import { useTranslation } from 'src/hooks/useTranslation';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { subjectsApi } from 'src/lib/api/subjects/subjectsApi';
import type { ISubject, ISubjectInput } from 'src/lib/api/subjects/subjectsTypes';
import {
    renderSubjectIcon,
    subjectColors,
    subjectIcons,
    subjectLanguages,
} from 'src/lib/subjects/subjectStyles';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** Mount it only while open (the form starts from the props). */
interface IProps {
    open: boolean;
    /** Edit this subject; without it a new one is created. */
    subject?: ISubject;
    onClose: () => void;
    onSaved: () => void;
}

const maxNameLength = 50;

const toForm = (subject?: ISubject): ISubjectInput => ({
    name: subject?.name ?? '',
    languageCode: subject?.languageCode ?? subjectLanguages[0].code,
    color: subject?.color ?? subjectColors[3].key,
    icon: subject?.icon ?? subjectIcons[3].key,
});

/** Create or edit a subject: name, language for reading aloud, color and icon from the fixed choice (LP-109). */
const SubjectDialog: React.FC<IProps> = (props) => {
    const { open, subject, onClose, onSaved } = props;
    const { getResource } = useTranslation();

    const [form, setForm] = React.useState<ISubjectInput>(() => toForm(subject));
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSaving, setIsSaving] = React.useState(false);

    const name = form.name.trim();
    const isValid = name.length > 0 && name.length <= maxNameLength;

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setErrorKey(null);

        const body = { ...form, name };
        const result = subject
            ? await subjectsApi.update.post({ body: { id: subject.id, ...body } })
            : await subjectsApi.create.post({ body });

        setIsSaving(false);

        if (result.error) {
            const errors = getFieldErrors(result.error.problem);
            setFieldErrors(errors);

            if (Object.keys(errors).length === 0) {
                setErrorKey(result.error.messageKey);
            }

            return;
        }

        onSaved();
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <form onSubmit={handleSubmit} noValidate>
                <DialogTitle>
                    {getResource(subject ? 'captionEditSubject' : 'captionAddSubject')}
                </DialogTitle>
                <DialogContent>
                    <Stack spacing={3} sx={{ pt: 1 }}>
                        {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

                        <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                            <SubjectBadge subject={{ ...form, name: name || '…' }} size="large" />
                        </Box>

                        <FormTextField
                            label={getResource('labelSubjectName')}
                            name="name"
                            required
                            value={form.name}
                            errorText={fieldErrors.name}
                            onChange={(value) => setForm({ ...form, name: value })}
                        />

                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelSubjectLanguage')}
                            </Typography>
                            <ToggleButtonGroup
                                exclusive
                                value={form.languageCode}
                                aria-label={getResource('labelSubjectLanguage')}
                                sx={{ flexWrap: 'wrap' }}
                                onChange={(_, languageCode: string | null) => {
                                    if (languageCode !== null) {
                                        setForm({ ...form, languageCode });
                                    }
                                }}
                            >
                                {subjectLanguages.map((language) => (
                                    <ToggleButton
                                        key={language.code}
                                        value={language.code}
                                        sx={{ minHeight: 56 }}
                                    >
                                        {getResource(language.labelKey)}
                                    </ToggleButton>
                                ))}
                            </ToggleButtonGroup>
                            {fieldErrors.languageCode && (
                                <Typography color="error" sx={{ mt: 1 }}>
                                    {fieldErrors.languageCode}
                                </Typography>
                            )}
                        </Box>

                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelSubjectColor')}
                            </Typography>
                            <Box
                                role="radiogroup"
                                aria-label={getResource('labelSubjectColor')}
                                sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}
                            >
                                {subjectColors.map((color) => {
                                    const isSelected = form.color === color.key;

                                    return (
                                        <IconButton
                                            key={color.key}
                                            role="radio"
                                            aria-checked={isSelected}
                                            aria-label={getResource(color.labelKey)}
                                            onClick={() => setForm({ ...form, color: color.key })}
                                            sx={{
                                                width: 56,
                                                height: 56,
                                                bgcolor: color.key,
                                                color: 'common.white',
                                                border: '3px solid',
                                                borderColor: isSelected
                                                    ? 'text.primary'
                                                    : 'transparent',
                                                '&:hover': { bgcolor: color.key, opacity: 0.85 },
                                            }}
                                        >
                                            {isSelected && <CheckIcon />}
                                        </IconButton>
                                    );
                                })}
                            </Box>
                        </Box>

                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelSubjectIcon')}
                            </Typography>
                            <Box
                                role="radiogroup"
                                aria-label={getResource('labelSubjectIcon')}
                                sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}
                            >
                                {subjectIcons.map((entry) => {
                                    const isSelected = form.icon === entry.key;

                                    return (
                                        <IconButton
                                            key={entry.key}
                                            role="radio"
                                            aria-checked={isSelected}
                                            aria-label={getResource(entry.labelKey)}
                                            onClick={() => setForm({ ...form, icon: entry.key })}
                                            sx={{
                                                width: 56,
                                                height: 56,
                                                border: '3px solid',
                                                borderColor: isSelected
                                                    ? 'primary.main'
                                                    : 'divider',
                                                color: isSelected
                                                    ? 'primary.main'
                                                    : 'text.secondary',
                                            }}
                                        >
                                            {renderSubjectIcon(entry.key)}
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
                        onClick={onClose}
                    />
                    <FormButton
                        label={getResource('labelSave')}
                        type="submit"
                        disabled={!isValid || isSaving}
                    />
                </DialogActions>
            </form>
        </Dialog>
    );
};

export default SubjectDialog;
