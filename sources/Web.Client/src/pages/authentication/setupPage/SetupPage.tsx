import React from 'react';
import { Alert, Stack } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import FormButton from 'src/components/input/FormButton';
import FormCheckbox from 'src/components/input/FormCheckbox';
import FormPasswordField from 'src/components/input/FormPasswordField';
import FormTextField from 'src/components/input/FormTextField';
import AuthCard from 'src/components/layout/AuthCard';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useForm } from 'src/hooks/useForm';
import { useSetupCompletion } from 'src/hooks/useSetupCompletion';
import { useTranslation } from 'src/hooks/useTranslation';
import { getFieldErrors } from 'src/lib/api/getFieldErrors';
import { setupApi } from 'src/lib/api/setup/setupApi';
import {
    getEmailError,
    getPasswordError,
    getPasswordRepeatError,
} from 'src/lib/auth/authValidation';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { utils } from 'src/lib/utils';
import { routes } from 'src/navigation/routes';

interface ISetupForm {
    familyName: string;
    displayName: string;
    email: string;
    password: string;
    passwordRepeat: string;
    privacyAccepted: boolean;
}

const initialForm: ISetupForm = {
    familyName: '',
    displayName: '',
    email: '',
    password: '',
    passwordRepeat: '',
    privacyAccepted: false,
};

const isSetupValid = (model: ISetupForm): boolean =>
    model.familyName.trim() !== '' &&
    model.displayName.trim() !== '' &&
    utils.validation.validateEmail(model.email.trim()) &&
    utils.validation.validatePassword(model.password) &&
    model.password === model.passwordRepeat &&
    model.privacyAccepted;

/** First start: creates the owner account and the family. Afterwards the user is signed in. */
const SetupPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { reloadUser } = useAuthentication();
    const { markSetupDone } = useSetupCompletion();
    const navigate = useNavigate();
    const { state, isValid, updateFormField } = useForm<ISetupForm>(initialForm, isSetupValid);

    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [fieldErrors, setFieldErrors] = React.useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    const [isPrivacyTouched, setIsPrivacyTouched] = React.useState(false);

    const update = (change: Partial<ISetupForm>) => {
        setFieldErrors({});
        updateFormField(change);
    };

    const text = (key: NotificationKey | undefined) => (key ? getResource(key) : undefined);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        setErrorKey(null);
        setFieldErrors({});
        setIsSubmitting(true);

        const result = await setupApi.initialize.post({
            body: {
                familyName: state.familyName.trim(),
                displayName: state.displayName.trim(),
                email: state.email.trim(),
                password: state.password,
                privacyAccepted: state.privacyAccepted,
            },
        });

        if (result.error) {
            setIsSubmitting(false);

            const errors = getFieldErrors(result.error.problem);
            setFieldErrors(errors);

            if (result.error.status === 409) {
                setErrorKey('notificationSetupAlreadyDone');
            } else if (Object.keys(errors).length === 0) {
                setErrorKey(result.error.messageKey);
            }

            return;
        }

        await reloadUser();
        // Tell the SetupGate first – otherwise it would send /admin straight back to /setup.
        markSetupDone();
        navigate(routes.admin, { replace: true });
    };

    return (
        <AuthCard
            title={getResource('captionSetup')}
            subtitle={getResource('captionSetupDescription')}
        >
            <form onSubmit={handleSubmit} noValidate>
                <Stack spacing={2}>
                    {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                    <FormTextField
                        label={getResource('labelFamilyName')}
                        name="familyName"
                        required
                        value={state.familyName}
                        errorText={fieldErrors.familyName}
                        onChange={(familyName) => update({ familyName })}
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
                    <FormTextField
                        label={getResource('labelEmail')}
                        name="email"
                        type="email"
                        required
                        value={state.email}
                        errorText={fieldErrors.email ?? text(getEmailError(state.email))}
                        onChange={(email) => update({ email })}
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
                        label={getResource('labelCreateFamily')}
                        type="submit"
                        disabled={!isValid || isSubmitting}
                    />
                </Stack>
            </form>
        </AuthCard>
    );
};

export default SetupPage;
