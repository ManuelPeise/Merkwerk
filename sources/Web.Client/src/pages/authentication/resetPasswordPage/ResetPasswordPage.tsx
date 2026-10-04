import React from 'react';
import { Alert, Button, Stack } from '@mui/material';
import { Link, useSearchParams } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import FormButton from 'src/components/input/FormButton';
import FormPasswordField from 'src/components/input/FormPasswordField';
import AuthCard from 'src/components/layout/AuthCard';
import { useForm } from 'src/hooks/useForm';
import { useTranslation } from 'src/hooks/useTranslation';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { getPasswordError, getPasswordRepeatError } from 'src/lib/auth/authValidation';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { utils } from 'src/lib/utils';
import { routes } from 'src/navigation/routes';

interface IResetPasswordForm {
    password: string;
    passwordRepeat: string;
}

const isResetPasswordValid = (model: IResetPasswordForm): boolean =>
    utils.validation.validatePassword(model.password) && model.password === model.passwordRepeat;

const resetPasswordCardUiTestId = uiTestId('auth-reset-password-card');

/** checking = asking the API whether the link can still be used (LP-166). */
type LinkState = 'checking' | 'valid' | 'invalid';

/** /reset-password?email=...&token=... - the link from the reset mail. */
const ResetPasswordPage: React.FC = () => {
    const { getResource } = useTranslation();
    const [searchParams] = useSearchParams();
    const email = searchParams.get('email');
    const token = searchParams.get('token');

    const { state, isValid, updateFormField } = useForm<IResetPasswordForm>(
        { password: '', passwordRepeat: '' },
        isResetPasswordValid,
    );

    const [linkState, setLinkState] = React.useState<LinkState>(
        email === null || token === null ? 'invalid' : 'checking',
    );
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [isDone, setIsDone] = React.useState(false);

    // Used or expired links say so right away instead of after the password was typed (LP-166).
    React.useEffect(() => {
        if (email === null || token === null) {
            return;
        }

        let isCurrent = true;

        void authenticationApi.verifyResetToken.post({ body: { email, token } }).then((result) => {
            if (isCurrent) {
                setLinkState(result.error ? 'invalid' : 'valid');
            }
        });

        return () => {
            isCurrent = false;
        };
    }, [email, token]);

    const update = (change: Partial<IResetPasswordForm>) => {
        setFieldErrors({});
        updateFormField(change);
    };

    const text = (key: NotificationKey | undefined) => (key ? getResource(key) : undefined);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        if (email === null || token === null) {
            return;
        }

        setErrorKey(null);
        setFieldErrors({});
        setIsSubmitting(true);

        const result = await authenticationApi.resetPassword.post({
            body: { email, token, newPassword: state.password },
        });

        setIsSubmitting(false);

        if (!result.error) {
            setIsDone(true);
            return;
        }

        const errors = getFieldErrors(result.error.problem);
        setFieldErrors(errors);

        if (Object.keys(errors).length === 0) {
            // 400 without field errors: token invalid or expired.
            setErrorKey(
                result.error.status === 400 ? 'notificationLinkExpired' : result.error.messageKey,
            );
        }
    };

    const toLoginButton = (
        <Button component={Link} to={routes.login} variant="contained">
            {getResource('labelToLogin')}
        </Button>
    );

    if (linkState === 'checking') {
        return (
            <AuthCard
                title={getResource('captionResetPassword')}
                uiTestId={resetPasswordCardUiTestId}
            >
                <LoadingIndicator uiTestId={uiTestId('loading-reset-password-page')} />
            </AuthCard>
        );
    }

    if (linkState === 'invalid' || email === null || token === null) {
        return (
            <AuthCard
                title={getResource('captionResetPassword')}
                uiTestId={resetPasswordCardUiTestId}
            >
                <Stack spacing={2}>
                    <Alert severity="error">{getResource('notificationLinkExpired')}</Alert>
                    {toLoginButton}
                </Stack>
            </AuthCard>
        );
    }

    return (
        <AuthCard title={getResource('captionResetPassword')} uiTestId={resetPasswordCardUiTestId}>
            {isDone ? (
                <Stack spacing={2}>
                    <Alert severity="success">{getResource('notificationPasswordChanged')}</Alert>
                    {toLoginButton}
                </Stack>
            ) : (
                <form onSubmit={handleSubmit} noValidate>
                    <Stack spacing={2}>
                        {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                        <FormPasswordField
                            label={getResource('labelPassword')}
                            name="password"
                            autoComplete="new-password"
                            required
                            value={state.password}
                            errorText={
                                fieldErrors.newPassword ?? text(getPasswordError(state.password))
                            }
                            uiTestId={uiTestIdOf(resetPasswordCardUiTestId, 'password')}
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
                            uiTestId={uiTestIdOf(resetPasswordCardUiTestId, 'password-repeat')}
                            onChange={(passwordRepeat) => update({ passwordRepeat })}
                        />
                        <FormButton
                            label={getResource('labelSetPassword')}
                            type="submit"
                            disabled={!isValid || isSubmitting}
                            uiTestId={uiTestIdOf(resetPasswordCardUiTestId, 'submit')}
                        />
                    </Stack>
                </form>
            )}
        </AuthCard>
    );
};

export default ResetPasswordPage;
