import React from 'react';
import {
    Alert,
    Box,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Stack,
    Typography,
} from '@mui/material';
import FormButton from 'src/components/input/FormButton';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormTextField from 'src/components/input/FormTextField';
import AvatarImage from 'src/components/kids/AvatarImage';
import { useTranslation } from 'src/hooks/useTranslation';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { groupsApi } from 'src/lib/api/groups/groupsApi';
import type { IGroup } from 'src/lib/api/groups/groupsTypes';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/** Mount it only while open (the form starts from the props). */
interface IProps {
    open: boolean;
    /** Edit this group; without it a new one is created. */
    group?: IGroup;
    /** All children of the family to choose from. */
    learners: ILearner[];
    onClose: () => void;
    onSaved: () => void;
}

const maxNameLength = 50;

/** Create or edit a group: name and which children belong to it (LP-108). */
const GroupDialog: React.FC<IProps> = (props) => {
    const { open, group, learners, onClose, onSaved } = props;
    const { getResource } = useTranslation();

    const [name, setName] = React.useState(group?.name ?? '');
    const [learnerIds, setLearnerIds] = React.useState<number[]>(group?.learnerIds ?? []);
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSaving, setIsSaving] = React.useState(false);

    const trimmedName = name.trim();
    const isValid = trimmedName.length > 0 && trimmedName.length <= maxNameLength;

    const toggleLearner = (learnerId: number, checked: boolean) =>
        setLearnerIds((ids) =>
            checked ? [...ids, learnerId] : ids.filter((id) => id !== learnerId),
        );

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsSaving(true);
        setErrorKey(null);

        const body = { name: trimmedName, learnerIds };
        const result = group
            ? await groupsApi.update.post({ body: { id: group.id, ...body } })
            : await groupsApi.create.post({ body });

        setIsSaving(false);

        if (result.error) {
            const errors = getFieldErrors(result.error.problem);
            setFieldErrors(errors);

            if (result.error.status === 409) {
                setErrorKey('notificationGroupLimitReached');
            } else if (errors.learnerIds) {
                // A child was deleted meanwhile – the list is outdated.
                setErrorKey('notificationGroupChildrenChanged');
            } else if (Object.keys(errors).length === 0) {
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
                    {getResource(group ? 'captionEditGroup' : 'captionAddGroup')}
                </DialogTitle>
                <DialogContent>
                    <Stack spacing={3} sx={{ pt: 1 }}>
                        {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                        <FormTextField
                            label={getResource('labelGroupName')}
                            name="name"
                            required
                            value={name}
                            errorText={fieldErrors.name}
                            helperText={getResource('captionGroupNameHint')}
                            onChange={setName}
                        />
                        <Box>
                            <Typography component="p" sx={{ fontWeight: 700, mb: 1 }}>
                                {getResource('labelGroupChildren')}
                            </Typography>
                            {learners.length === 0 ? (
                                <Typography color="text.secondary">
                                    {getResource('captionGroupNoChildren')}
                                </Typography>
                            ) : (
                                <Stack spacing={0.5}>
                                    {learners.map((learner) => (
                                        <FormCheckbox
                                            key={learner.id}
                                            name={`learner-${learner.id}`}
                                            checked={learnerIds.includes(learner.id)}
                                            onChange={(checked) =>
                                                toggleLearner(learner.id, checked)
                                            }
                                            label={
                                                <Box
                                                    component="span"
                                                    sx={{
                                                        display: 'inline-flex',
                                                        alignItems: 'center',
                                                        gap: 1.5,
                                                    }}
                                                >
                                                    <AvatarImage
                                                        avatarId={learner.avatarId}
                                                        name={learner.displayName}
                                                        size={40}
                                                    />
                                                    {learner.displayName}
                                                </Box>
                                            }
                                        />
                                    ))}
                                </Stack>
                            )}
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

export default GroupDialog;
