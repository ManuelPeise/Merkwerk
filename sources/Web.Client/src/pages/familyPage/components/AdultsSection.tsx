import React from 'react';
import {
    Alert,
    Box,
    Card,
    Chip,
    IconButton,
    List,
    ListItem,
    ListItemText,
    Stack,
    ToggleButton,
    ToggleButtonGroup,
    Typography,
} from '@mui/material';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { DeleteIcon, ResetPasswordIcon } from 'src/components/icons/AppIcons';
import FormButton from 'src/components/input/FormButton';
import FormTextField from 'src/components/input/FormTextField';
import { useTranslation } from 'src/hooks/useTranslation';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { invitationsApi } from 'src/lib/api/invitations/invitationsApi';
import type { IInvitation } from 'src/lib/api/invitations/invitationsTypes';
import { membersApi } from 'src/lib/api/members/membersApi';
import type { IMember, OrganizationRole } from 'src/lib/api/members/membersTypes';
import type { LabelKey, NotificationKey } from 'src/lib/translations/translationKeys';
import { utils } from 'src/lib/utils';
import ConfirmDialog from 'src/pages/familyPage/components/ConfirmDialog';

interface IProps {
    isAdmin: boolean;
}

type Feedback = { severity: 'success' | 'error'; key: NotificationKey; values?: Record<string, string> };

type ConfirmState =
    | { kind: 'none' }
    | { kind: 'remove'; member: IMember }
    | { kind: 'resetPassword'; member: IMember };

const roleLabel: Record<OrganizationRole, LabelKey> = {
    Member: 'labelRoleMember',
    OrgAdmin: 'labelRoleOrgAdmin',
};

/** Adults of the family; admins invite, withdraw invitations, remove adults and send start passwords. */
const AdultsSection: React.FC<IProps> = (props) => {
    const { isAdmin } = props;
    const { getResource, language } = useTranslation();

    const [members, setMembers] = React.useState<IMember[] | null>(null);
    const [invitations, setInvitations] = React.useState<IInvitation[]>([]);
    const [feedback, setFeedback] = React.useState<Feedback | null>(null);
    const [confirm, setConfirm] = React.useState<ConfirmState>({ kind: 'none' });
    const [isBusy, setIsBusy] = React.useState(false);

    const [email, setEmail] = React.useState('');
    const [role, setRole] = React.useState<OrganizationRole>('Member');
    const [emailError, setEmailError] = React.useState<string | undefined>(undefined);

    const load = React.useCallback(
        async (signal?: AbortSignal) => {
            const membersResult = await membersApi.list.get({ signal });

            if (membersResult.error?.kind === 'canceled') {
                return;
            }

            setMembers(membersResult.data ?? []);

            if (membersResult.error) {
                setFeedback({ severity: 'error', key: membersResult.error.messageKey });
            }

            if (isAdmin) {
                const invitationsResult = await invitationsApi.list.get({ signal });
                setInvitations(invitationsResult.data ?? []);
            }
        },
        [isAdmin],
    );

    React.useEffect(() => {
        const controller = new AbortController();
        void load(controller.signal);
        return () => controller.abort();
    }, [load]);

    const formatDate = (value: string) => new Date(value).toLocaleDateString(language);

    const handleInvite = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setIsBusy(true);
        setFeedback(null);
        setEmailError(undefined);

        const address = email.trim();
        const result = await invitationsApi.create.post({ body: { email: address, role } });
        setIsBusy(false);

        if (result.error) {
            const errors = getFieldErrors(result.error.problem);
            setEmailError(errors.email);

            if (result.error.status === 409) {
                setFeedback({ severity: 'error', key: 'notificationAlreadyMember' });
            } else if (!errors.email) {
                setFeedback({ severity: 'error', key: result.error.messageKey });
            }

            return;
        }

        setEmail('');
        setFeedback({ severity: 'success', key: 'notificationInvitationSent', values: { email: address } });
        void load();
    };

    const handleRevoke = async (invitation: IInvitation) => {
        const result = await invitationsApi.revoke.post({ body: { id: invitation.id } });
        setFeedback(result.error ? { severity: 'error', key: result.error.messageKey } : null);
        void load();
    };

    const handleConfirm = async () => {
        if (confirm.kind === 'none') {
            return;
        }

        setIsBusy(true);
        const result =
            confirm.kind === 'remove'
                ? await membersApi.remove.post({ body: { membershipId: confirm.member.membershipId } })
                : await authenticationApi.adminResetPassword.post({ body: { userId: confirm.member.userId } });
        setIsBusy(false);

        if (result.error) {
            setFeedback({
                severity: 'error',
                key: result.error.status === 409 ? 'notificationMemberCannotBeRemoved' : result.error.messageKey,
            });
        } else {
            setFeedback({
                severity: 'success',
                key: confirm.kind === 'remove' ? 'notificationMemberRemoved' : 'notificationStartPasswordSent',
            });
        }

        setConfirm({ kind: 'none' });
        void load();
    };

    const isEmailValid = utils.validation.validateEmail(email.trim());

    return (
        <Stack spacing={2} component="section" aria-labelledby="adults-heading">
            <Typography variant="h2" id="adults-heading">
                {getResource('captionAdults')}
            </Typography>

            {feedback && (
                <Alert severity={feedback.severity} onClose={() => setFeedback(null)}>
                    {getResource(feedback.key, feedback.values)}
                </Alert>
            )}

            {members === null ? (
                <LoadingIndicator />
            ) : (
                <Card>
                    <List>
                        {members.map((member) => (
                            <ListItem
                                key={member.membershipId}
                                secondaryAction={
                                    isAdmin && (
                                        <Box sx={{ display: 'flex', gap: 1 }}>
                                            <IconButton
                                                aria-label={getResource('labelResetPassword', {
                                                    name: member.displayName,
                                                })}
                                                onClick={() => setConfirm({ kind: 'resetPassword', member })}
                                            >
                                                <ResetPasswordIcon />
                                            </IconButton>
                                            {!member.isOwner && (
                                                <IconButton
                                                    aria-label={getResource('labelRemoveMember', {
                                                        name: member.displayName,
                                                    })}
                                                    onClick={() => setConfirm({ kind: 'remove', member })}
                                                >
                                                    <DeleteIcon />
                                                </IconButton>
                                            )}
                                        </Box>
                                    )
                                }
                            >
                                <ListItemText
                                    primary={
                                        <Box component="span" sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                                            <span>{member.displayName}</span>
                                            <Chip size="small" label={getResource(roleLabel[member.role])} />
                                            {member.isOwner && (
                                                <Chip size="small" variant="outlined" label={getResource('labelOwner')} />
                                            )}
                                        </Box>
                                    }
                                    secondary={member.email}
                                />
                            </ListItem>
                        ))}
                    </List>
                </Card>
            )}

            {isAdmin && (
                <>
                    <Typography variant="h2" component="h3" sx={{ fontSize: '1.375rem', pt: 2 }}>
                        {getResource('captionInviteAdult')}
                    </Typography>
                    <form onSubmit={handleInvite} noValidate>
                        <Stack spacing={2}>
                            <FormTextField
                                label={getResource('labelEmail')}
                                name="email"
                                type="email"
                                required
                                value={email}
                                errorText={emailError}
                                onChange={(value) => {
                                    setEmail(value);
                                    setEmailError(undefined);
                                }}
                            />
                            <ToggleButtonGroup
                                exclusive
                                value={role}
                                aria-label={getResource('labelRole')}
                                onChange={(_, value: OrganizationRole | null) => {
                                    if (value) {
                                        setRole(value);
                                    }
                                }}
                            >
                                <ToggleButton value="Member" sx={{ minHeight: 56 }}>
                                    {getResource('labelRoleMember')}
                                </ToggleButton>
                                <ToggleButton value="OrgAdmin" sx={{ minHeight: 56 }}>
                                    {getResource('labelRoleOrgAdmin')}
                                </ToggleButton>
                            </ToggleButtonGroup>
                            <Box>
                                <FormButton
                                    label={getResource('labelInvite')}
                                    type="submit"
                                    disabled={!isEmailValid || isBusy}
                                />
                            </Box>
                        </Stack>
                    </form>

                    <Typography variant="h2" component="h3" sx={{ fontSize: '1.375rem', pt: 2 }}>
                        {getResource('captionInvitations')}
                    </Typography>
                    {invitations.length === 0 ? (
                        <Typography color="text.secondary">{getResource('captionNoInvitations')}</Typography>
                    ) : (
                        <Card>
                            <List>
                                {invitations.map((invitation) => (
                                    <ListItem
                                        key={invitation.id}
                                        secondaryAction={
                                            <IconButton
                                                aria-label={getResource('labelRevokeInvitation', {
                                                    email: invitation.email,
                                                })}
                                                onClick={() => void handleRevoke(invitation)}
                                            >
                                                <DeleteIcon />
                                            </IconButton>
                                        }
                                    >
                                        <ListItemText
                                            primary={invitation.email}
                                            secondary={
                                                invitation.state === 'Expired'
                                                    ? getResource('captionInvitationExpired')
                                                    : getResource('captionInvitationValidUntil', {
                                                          date: formatDate(invitation.expiresAt),
                                                      })
                                            }
                                        />
                                    </ListItem>
                                ))}
                            </List>
                        </Card>
                    )}
                </>
            )}

            {confirm.kind !== 'none' && (
                <ConfirmDialog
                    open
                    title={getResource(
                        confirm.kind === 'remove' ? 'labelRemoveMember' : 'labelResetPassword',
                        { name: confirm.member.displayName },
                    )}
                    text={getResource(
                        confirm.kind === 'remove' ? 'captionConfirmRemoveMember' : 'captionConfirmResetPassword',
                        { name: confirm.member.displayName },
                    )}
                    confirmLabel={getResource(
                        confirm.kind === 'remove' ? 'labelRemove' : 'labelSendStartPassword',
                    )}
                    disabled={isBusy}
                    onConfirm={() => void handleConfirm()}
                    onCancel={() => setConfirm({ kind: 'none' })}
                />
            )}
        </Stack>
    );
};

export default AdultsSection;
