import React from 'react';
import {
    Alert,
    Box,
    Card,
    IconButton,
    List,
    ListItem,
    ListItemAvatar,
    ListItemText,
    Stack,
    Typography,
} from '@mui/material';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { DeleteIcon, EditIcon } from 'src/components/icons/AppIcons';
import AvatarImage from 'src/components/kids/AvatarImage';
import FormButton from 'src/components/input/FormButton';
import { useTranslation } from 'src/hooks/useTranslation';
import { learnersApi } from 'src/lib/api/learners/learnersApi';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import ConfirmDialog from 'src/components/feedback/ConfirmDialog';
import LearnerDialog from 'src/pages/familyPage/components/LearnerDialog';

interface IProps {
    isAdmin: boolean;
}

type DialogState = { kind: 'closed' } | { kind: 'edit'; learner?: ILearner } | { kind: 'delete'; learner: ILearner };

/** Children of the family: list, and for admins add, edit and delete. */
const LearnersSection: React.FC<IProps> = (props) => {
    const { isAdmin } = props;
    const { getResource } = useTranslation();

    const [learners, setLearners] = React.useState<ILearner[] | null>(null);
    const [loadErrorKey, setLoadErrorKey] = React.useState<NotificationKey | null>(null);
    const [actionErrorKey, setActionErrorKey] = React.useState<NotificationKey | null>(null);
    const [dialog, setDialog] = React.useState<DialogState>({ kind: 'closed' });
    const [isDeleting, setIsDeleting] = React.useState(false);
    // Incremented to reload the list after a change.
    const [reloadKey, setReloadKey] = React.useState(0);

    React.useEffect(() => {
        const controller = new AbortController();

        void learnersApi.list.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setLearners(result.data ?? []);
            setLoadErrorKey(result.error ? result.error.messageKey : null);
        });

        return () => controller.abort();
    }, [reloadKey]);

    const reload = () => setReloadKey((key) => key + 1);

    const handleSaved = () => {
        setDialog({ kind: 'closed' });
        setActionErrorKey(null);
        reload();
    };

    const handleDelete = async (learner: ILearner) => {
        setIsDeleting(true);
        const result = await learnersApi.delete.post({ body: { id: learner.id } });
        setIsDeleting(false);
        setDialog({ kind: 'closed' });
        setActionErrorKey(result.error ? result.error.messageKey : null);
        reload();
    };

    const errorKey = actionErrorKey ?? loadErrorKey;

    return (
        <Stack spacing={2} component="section" aria-labelledby="children-heading">
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
                <Typography variant="h2" id="children-heading">
                    {getResource('captionChildren')}
                </Typography>
                {isAdmin && (
                    <FormButton
                        label={getResource('labelAddChild')}
                        onClick={() => setDialog({ kind: 'edit' })}
                    />
                )}
            </Box>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            {learners === null ? (
                <LoadingIndicator />
            ) : learners.length === 0 ? (
                <Typography color="text.secondary">{getResource('captionNoChildren')}</Typography>
            ) : (
                <Card>
                    <List>
                        {learners.map((learner) => (
                            <ListItem
                                key={learner.id}
                                secondaryAction={
                                    isAdmin && (
                                        <Box sx={{ display: 'flex', gap: 1 }}>
                                            <IconButton
                                                aria-label={getResource('labelEditChild', {
                                                    name: learner.displayName,
                                                })}
                                                onClick={() => setDialog({ kind: 'edit', learner })}
                                            >
                                                <EditIcon />
                                            </IconButton>
                                            <IconButton
                                                aria-label={getResource('labelDeleteChild', {
                                                    name: learner.displayName,
                                                })}
                                                onClick={() => setDialog({ kind: 'delete', learner })}
                                            >
                                                <DeleteIcon />
                                            </IconButton>
                                        </Box>
                                    )
                                }
                            >
                                <ListItemAvatar sx={{ mr: 2 }}>
                                    <AvatarImage
                                        avatarId={learner.avatarId}
                                        name={learner.displayName}
                                        size={48}
                                    />
                                </ListItemAvatar>
                                <ListItemText
                                    primary={learner.displayName}
                                    secondary={getResource('captionGrade', { grade: learner.grade })}
                                />
                            </ListItem>
                        ))}
                    </List>
                </Card>
            )}

            {dialog.kind === 'edit' && (
                <LearnerDialog
                    open
                    learner={dialog.learner}
                    onClose={() => setDialog({ kind: 'closed' })}
                    onSaved={handleSaved}
                />
            )}
            {dialog.kind === 'delete' && (
                <ConfirmDialog
                    open
                    title={getResource('labelDeleteChild', { name: dialog.learner.displayName })}
                    text={getResource('captionConfirmDeleteChild', { name: dialog.learner.displayName })}
                    confirmLabel={getResource('labelDelete')}
                    disabled={isDeleting}
                    onConfirm={() => void handleDelete(dialog.learner)}
                    onCancel={() => setDialog({ kind: 'closed' })}
                />
            )}
        </Stack>
    );
};

export default LearnersSection;
