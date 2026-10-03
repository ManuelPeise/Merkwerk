import React from 'react';
import { Alert, Stack } from '@mui/material';
import { useNavigate, useSearchParams } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import FormButton from 'src/components/input/FormButton';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormPasswordField from 'src/components/input/FormPasswordField';
import FormTextField from 'src/components/input/FormTextField';
import AuthCard from 'src/components/layout/AuthCard';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useForm } from 'src/hooks/useForm';
import { useTranslation } from 'src/hooks/useTranslation';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { invitationsApi } from 'src/lib/api/invitations/invitationsApi';
import type { IInvitationDetails } from 'src/lib/api/invitations/invitationsTypes';
import { getPasswordError, getPasswordRepeatError } from 'src/lib/auth/authValidation';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { utils } from 'src/lib/utils';
import { routes } from 'src/navigation/routes';

interface IInvitationForm {
    displayName: string;
    password: string;
    passwordRepeat: string;
    privacyAccepted: boolean;
}

const initialForm: IInvitationForm = {
    displayName: '',
    password: '',
    passwordRepeat: '',
    privacyAccepted: false,
};

const isInvitationValid = (model: IInvitationForm): boolean =>
    model.displayName.trim() !== '' &&
    utils.validation.validatePassword(model.password) &&
    model.password === model.passwordRepeat &&
    model.privacyAccepted;

/** /invitation?token=… – an invited adult joins an existing family and is signed in afterwards. */
const InvitationPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { reloadUser } = useAuthentication();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const token = searchParams.get('token');

    const { state, isValid, updateFormField } = useForm<IInvitationForm>(
        initialForm,
        isInvitationValid,
    );

    const [invitation, setInvitation] = React.useState<IInvitationDetails | null>(null);
    const [isLoading, setIsLoading] = React.useState(token !== null);
    const [loadErrorKey, setLoadErrorKey] = React.useState<NotificationKey | null>(
        token === null ? 'notificationInvitationInvalid' : null,
    );
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [isPrivacyTouched, setIsPrivacyTouched] = React.useState(false);

    React.useEffect(() => {
        if (token === null) {
            return;
        }

        const controller = new AbortController();

        void invitationsApi.details
            .get({ params: { token }, signal: controller.signal })
            .then((result) => {
                if (result.error?.kind === 'canceled') {
                    return;
                }

                if (result.data) {
                    setInvitation(result.data);
                } else {
                    setLoadErrorKey(
                        result.error.status === 404
                            ? 'notificationInvitationInvalid'
                            : result.error.messageKey,
                    );
                }

                setIsLoading(false);
            });

        return () => controller.abort();
    }, [token]);

    const update = (change: Partial<IInvitationForm>) => {
        setFieldErrors({});
        updateFormField(change);
    };

    const text = (key: NotificationKey | undefined) => (key ? getResource(key) : undefined);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        if (token === null) {
            return;
        }

        setErrorKey(null);
        setFieldErrors({});
        setIsSubmitting(true);

        const result = await invitationsApi.accept.post({
            body: {
                token,
                displayName: state.displayName.trim(),
                password: state.password,
                privacyAccepted: state.privacyAccepted,
            },
        });

        if (result.error) {
            setIsSubmitting(false);

            const errors = getFieldErrors(result.error.problem);
            setFieldErrors(errors);

            if (Object.keys(errors).length === 0) {
                setErrorKey(result.error.messageKey);
            }

            return;
        }

        await reloadUser();
        navigate(routes.admin, { replace: true });
    };

    if (isLoading) {
        return <LoadingIndicator />;
    }

    if (loadErrorKey || !invitation) {
        return (
            <AuthCard title={getResource('captionInvitation')}>
                <Alert severity="error">
                    {getResource(loadErrorKey ?? 'notificationInvitationInvalid')}
                </Alert>
            </AuthCard>
        );
    }

    return (
        <AuthCard
            title={getResource('captionInvitation')}
            subtitle={getResource('captionInvitationFor', { familyName: invitation.familyName })}
        >
            <form onSubmit={handleSubmit} noValidate>
                <Stack spacing={2}>
                    {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                    <FormTextField
                        label={getResource('labelEmail')}
                        name="email"
                        type="email"
                        disabled
                        value={invitation.email}
                        onChange={() => undefined}
                    />
                    <FormTextField
                        label={getResource('labelDisplayName')}
                        name="displayName"
                        autoComplete="name"
                        required
                        value={state.displayName}
                        errorText={fieldErrors.displayName}
                        onChange={(displayName) => update({ displayName })}
                    />
                    <FormPasswordField
                        label={getResource('labelPassword')}
                        name="password"
                        autoComplete="new-password"
                        required
                        value={state.password}
                        errorText={fieldErrors.password ?? text(getPasswordError(state.password))}
                        onChange={(password) => update({ password })}
                    />
                    <FormPasswordField
                        label={getResource('labelPasswordRepeat')}
                        name="passwordRepeat"
                        autoComplete="new-password"
                        required
                        value={state.passwordRepeat}
                        errorText={text(
                            getPasswordRepeatError(state.password, state.passwordRepeat),
                        )}
                        onChange={(passwordRepeat) => update({ passwordRepeat })}
                    />
                    <FormCheckbox
                        label={getResource('labelPrivacyAccept')}
                        name="privacyAccepted"
                        required
                        checked={state.privacyAccepted}
                        errorText={
                            fieldErrors.privacyAccepted ??
                            (isPrivacyTouched && !state.privacyAccepted
                                ? getResource('notificationPrivacyRequired')
                                : undefined)
                        }
                        onChange={(privacyAccepted) => {
                            setIsPrivacyTouched(true);
                            update({ privacyAccepted });
                        }}
                    />
                    <FormButton
                        label={getResource('labelAcceptInvitation')}
                        type="submit"
                        disabled={!isValid || isSubmitting}
                    />
                </Stack>
            </form>
        </AuthCard>
    );
};

export default InvitationPage;
