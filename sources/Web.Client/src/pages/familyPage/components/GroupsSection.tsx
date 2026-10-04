import React from 'react';
import {
    Alert,
    AvatarGroup,
    Box,
    Card,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Stack,
    Typography,
} from '@mui/material';
import ConfirmDialog from 'src/components/feedback/ConfirmDialog';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { DeleteIcon, EditIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import AvatarImage from 'src/components/kids/AvatarImage';
import { useTranslation } from 'src/hooks/useTranslation';
import { groupsApi } from 'src/lib/api/groups/groupsApi';
import type { IGroup } from 'src/lib/api/groups/groupsTypes';
import { learnersApi } from 'src/lib/api/learners/learnersApi';
import type { ILearner } from 'src/lib/api/learners/learnersTypes';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import GroupDialog from 'src/pages/familyPage/components/GroupDialog';

interface IProps {
    isAdmin: boolean;
    /** Changes when the children changed elsewhere on the page, so names and avatars stay current. */
    learnersVersion: number;
}

type DialogState =
    { kind: 'closed' } | { kind: 'edit'; group?: IGroup } | { kind: 'delete'; group: IGroup };

/** Groups of children (LP-108): list with the children's avatars; admins add, edit and delete. */
const GroupsSection: React.FC<IProps> = (props) => {
    const { isAdmin, learnersVersion } = props;
    const { getResource } = useTranslation();

    const [groups, setGroups] = React.useState<IGroup[] | null>(null);
    const [learners, setLearners] = React.useState<ILearner[]>([]);
    const [loadErrorKey, setLoadErrorKey] = React.useState<NotificationKey | null>(null);
    const [actionErrorKey, setActionErrorKey] = React.useState<NotificationKey | null>(null);
    const [dialog, setDialog] = React.useState<DialogState>({ kind: 'closed' });
    const [isDeleting, setIsDeleting] = React.useState(false);
    // Incremented to reload the list after a change.
    const [reloadKey, setReloadKey] = React.useState(0);

    React.useEffect(() => {
        const controller = new AbortController();

        void Promise.all([
            groupsApi.list.get({ signal: controller.signal }),
            learnersApi.list.get({ signal: controller.signal }),
        ]).then(([groupsResult, learnersResult]) => {
            if (
                groupsResult.error?.kind === 'canceled' ||
                learnersResult.error?.kind === 'canceled'
            ) {
                return;
            }

            setGroups(groupsResult.data ?? []);
            setLearners(learnersResult.data ?? []);
            const error = groupsResult.error ?? learnersResult.error;
            setLoadErrorKey(error ? error.messageKey : null);
        });

        return () => controller.abort();
    }, [reloadKey, learnersVersion]);

    const reload = () => setReloadKey((key) => key + 1);

    const handleSaved = () => {
        setDialog({ kind: 'closed' });
        setActionErrorKey(null);
        reload();
    };

    const handleDelete = async (group: IGroup) => {
        setIsDeleting(true);
        const result = await groupsApi.delete.post({ body: { id: group.id } });
        setIsDeleting(false);
        setDialog({ kind: 'closed' });
        setActionErrorKey(result.error ? result.error.messageKey : null);
        reload();
    };

    const membersOf = (group: IGroup): ILearner[] =>
        learners.filter((learner) => group.learnerIds.includes(learner.id));

    const errorKey = actionErrorKey ?? loadErrorKey;

    return (
        <Stack
            spacing={2}
            component="section"
            aria-labelledby="groups-heading"
            data-testid={uiTestId('family-groups-section')}
        >
            <Box
                sx={{
                    display: 'flex',
                    flexWrap: 'wrap',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    gap: 2,
                }}
            >
                <Typography variant="h2" id="groups-heading">
                    {getResource('captionGroups')}
                </Typography>
                {isAdmin && (
                    <FormButton
                        label={getResource('labelAddGroup')}
                        uiTestId={uiTestId('family-add-group-button')}
                        onClick={() => setDialog({ kind: 'edit' })}
                    />
                )}
            </Box>

            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}

            {groups === null ? (
                <LoadingIndicator uiTestId={uiTestId('loading-family-groups-section')} />
            ) : groups.length === 0 ? (
                <Typography color="text.secondary">{getResource('captionNoGroups')}</Typography>
            ) : (
                <Card>
                    <List>
                        {groups.map((group) => {
                            const members = membersOf(group);

                            return (
                                <ListItem
                                    key={group.id}
                                    data-testid={uiTestIdOf(
                                        uiTestId('family-group-item'),
                                        String(group.id),
                                    )}
                                    secondaryAction={
                                        isAdmin && (
                                            <Box sx={{ display: 'flex', gap: 1 }}>
                                                <IconButton
                                                    aria-label={getResource('labelEditGroup', {
                                                        name: group.name,
                                                    })}
                                                    onClick={() =>
                                                        setDialog({ kind: 'edit', group })
                                                    }
                                                >
                                                    <EditIcon />
                                                </IconButton>
                                                <IconButton
                                                    aria-label={getResource('labelDeleteGroup', {
                                                        name: group.name,
                                                    })}
                                                    onClick={() =>
                                                        setDialog({ kind: 'delete', group })
                                                    }
                                                >
                                                    <DeleteIcon />
                                                </IconButton>
                                            </Box>
                                        )
                                    }
                                >
                                    <ListItemText
                                        primary={group.name}
                                        secondary={
                                            members.length === 0
                                                ? getResource('captionGroupEmpty')
                                                : members
                                                      .map((learner) => learner.displayName)
                                                      .join(', ')
                                        }
                                    />
                                    {members.length > 0 && (
                                        <AvatarGroup max={6} sx={{ mr: isAdmin ? 12 : 0 }}>
                                            {members.map((learner) => (
                                                <AvatarImage
                                                    key={learner.id}
                                                    avatarId={learner.avatarId}
                                                    name={learner.displayName}
                                                    size={40}
                                                    uiTestId={uiTestIdOf(
                                                        uiTestId('family-group-member-avatar'),
                                                        String(learner.id),
                                                    )}
                                                />
                                            ))}
                                        </AvatarGroup>
                                    )}
                                </ListItem>
                            );
                        })}
                    </List>
                </Card>
            )}

            {dialog.kind === 'edit' && (
                <GroupDialog
                    open
                    group={dialog.group}
                    learners={learners}
                    onClose={() => setDialog({ kind: 'closed' })}
                    onSaved={handleSaved}
                    uiTestId={uiTestId('family-group-edit-dialog')}
                />
            )}
            {dialog.kind === 'delete' && (
                <ConfirmDialog
                    open
                    title={getResource('labelDeleteGroup', { name: dialog.group.name })}
                    text={getResource('captionConfirmDeleteGroup', { name: dialog.group.name })}
                    confirmLabel={getResource('labelDelete')}
                    disabled={isDeleting}
                    uiTestId={uiTestId('family-group-delete-dialog')}
                    onConfirm={() => void handleDelete(dialog.group)}
                    onCancel={() => setDialog({ kind: 'closed' })}
                />
            )}
        </Stack>
    );
};

export default GroupsSection;
