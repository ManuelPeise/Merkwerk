import React from 'react';
import { Alert, Stack } from '@mui/material';
import FormButton from 'src/components/input/FormButton';
import FormLink from 'src/components/input/FormLink';
import FormTextField from 'src/components/input/FormTextField';
import AuthCard from 'src/components/layout/AuthCard';
import { useForm } from 'src/hooks/useForm';
import { useTranslation } from 'src/hooks/useTranslation';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import type { IForgotPasswordRequest } from 'src/lib/api/authentication/authenticationTypes';
import { getEmailError } from 'src/lib/auth/authValidation';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { utils } from 'src/lib/utils';
import { routes } from 'src/navigation/routes';

const isForgotPasswordValid = (model: IForgotPasswordRequest): boolean =>
    utils.validation.validateEmail(model.email.trim());

const forgotPasswordCardUiTestId = uiTestId('auth-forgot-password-card');

/** Always shows the same confirmation - it never reveals whether an account exists. */
const ForgotPasswordPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { state, isValid, updateFormField } = useForm<IForgotPasswordRequest>(
        { email: '' },
        isForgotPasswordValid,
    );

    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [isSent, setIsSent] = React.useState(false);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setErrorKey(null);
        setIsSubmitting(true);

        const result = await authenticationApi.forgotPassword.post({
            body: { email: state.email.trim() },
        });

        setIsSubmitting(false);

        // Only connection problems are shown; every answer of the server counts as "sent".
        if (
            result.error &&
            (result.error.kind === 'network' || result.error.kind === 'unexpected')
        ) {
            setErrorKey(result.error.messageKey);
            return;
        }

        setIsSent(true);
    };

    const emailErrorKey = getEmailError(state.email);

    return (
        <AuthCard
            title={getResource('captionForgotPassword')}
            uiTestId={forgotPasswordCardUiTestId}
        >
            {isSent ? (
                <Stack spacing={2}>
                    <Alert severity="success">{getResource('notificationResetLinkSent')}</Alert>
                    <FormLink
                        to={routes.login}
                        label={getResource('labelToLogin')}
                        uiTestId={uiTestIdOf(forgotPasswordCardUiTestId, 'to-login-link')}
                    />
                </Stack>
            ) : (
                <form onSubmit={handleSubmit} noValidate>
                    <Stack spacing={2}>
                        {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                        <FormTextField
                            label={getResource('labelEmail')}
                            name="email"
                            type="email"
                            required
                            value={state.email}
                            errorText={emailErrorKey ? getResource(emailErrorKey) : undefined}
                            uiTestId={uiTestIdOf(forgotPasswordCardUiTestId, 'email')}
                            onChange={(email) => updateFormField({ email })}
                        />
                        <FormButton
                            label={getResource('labelSendResetLink')}
                            type="submit"
                            disabled={!isValid || isSubmitting}
                            uiTestId={uiTestIdOf(forgotPasswordCardUiTestId, 'submit')}
                        />
                        <FormLink
                            to={routes.login}
                            label={getResource('labelToLogin')}
                            uiTestId={uiTestIdOf(forgotPasswordCardUiTestId, 'to-login-link')}
                        />
                    </Stack>
                </form>
            )}
        </AuthCard>
    );
};

export default ForgotPasswordPage;
