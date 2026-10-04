import React from 'react';
import { Alert, Stack } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import AuthCard from 'src/components/layout/AuthCard';
import FormLink from 'src/components/input/FormLink';
import FormButton from 'src/components/input/FormButton';
import FormPasswordField from 'src/components/input/FormPasswordField';
import FormTextField from 'src/components/input/FormTextField';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useForm } from 'src/hooks/useForm';
import type { ILoginRequest } from 'src/lib/api/authentication/authenticationTypes';
import { problemTitles } from 'src/lib/api/problemTitles';
import type { ApiError } from 'src/lib/api/types/apiError';
import { testIds } from 'src/lib/testing/testIds';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { getEmailError } from 'src/lib/auth/authValidation';
import { routes } from 'src/navigation/routes';
import { utils } from 'src/lib/utils';

const isLoginValid = (model: ILoginRequest): boolean =>
    utils.validation.validateEmail(model.email.trim()) && model.password !== '';

/** 401 here means wrong credentials, not an expired session; 403 says why the correct password was not enough. */
const getLoginErrorKey = (error: ApiError): NotificationKey => {
    if (error.status === 401) {
        return 'notificationLoginFailed';
    }

    if (error.status === 403 && error.problem?.title === problemTitles.noMembership) {
        return 'notificationNoMembership';
    }

    if (error.status === 403 && error.problem?.title === problemTitles.emailNotConfirmed) {
        return 'notificationEmailNotConfirmed';
    }

    return error.messageKey;
};

/** After a successful login, PublicRoute sends the user on automatically. */
const LoginPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { login } = useAuthentication();
    const { state, isValid, updateFormField } = useForm<ILoginRequest>(
        { email: '', password: '' },
        isLoginValid,
    );

    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSubmitting, setIsSubmitting] = React.useState(false);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setErrorKey(null);
        setIsSubmitting(true);

        const error = await login(state);

        setIsSubmitting(false);

        if (error) {
            setErrorKey(getLoginErrorKey(error));
        }
    };

    const emailErrorKey = getEmailError(state.email);

    return (
        <AuthCard title={getResource('captionLogin')} testId={testIds.auth.login}>
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
                        onChange={(email) => updateFormField({ email })}
                    />
                    <FormPasswordField
                        label={getResource('labelPassword')}
                        name="password"
                        autoComplete="current-password"
                        required
                        value={state.password}
                        onChange={(password) => updateFormField({ password })}
                    />
                    <FormLink
                        to={routes.forgotPassword}
                        label={getResource('labelForgotPassword')}
                    />
                    <FormButton
                        label={getResource('labelLogin')}
                        type="submit"
                        disabled={!isValid || isSubmitting}
                    />
                </Stack>
            </form>
        </AuthCard>
    );
};

export default LoginPage;
