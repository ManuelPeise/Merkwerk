import React from 'react';
import { Alert, Stack } from '@mui/material';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import FormButton from 'src/components/input/FormButton';
import FormPinField from 'src/components/input/FormPinField';
import FormTextField from 'src/components/input/FormTextField';
import AuthCard from 'src/components/layout/AuthCard';
import { useTranslation } from 'src/hooks/useTranslation';
import { devicesApi } from 'src/lib/api/devices/devicesApi';
import { testIds } from 'src/lib/testing/testIds';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { routes, type IPairState } from 'src/navigation/routes';

const codeLength = 6;
const codePattern = /^\d{6}$/;

/** /practice/pair – an adult enters the pairing code. With ?code= (QR code) it pairs without typing. */
const PairDevicePage: React.FC = () => {
    const { getResource } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const [searchParams] = useSearchParams();

    const defaultDeviceName = getResource('labelDefaultDeviceName');
    const initialCode = searchParams.get('code') ?? '';
    const notice = (location.state as IPairState | null)?.notice;

    const [code, setCode] = React.useState(initialCode.replace(/\D/g, '').slice(0, codeLength));
    const [deviceName, setDeviceName] = React.useState(defaultDeviceName);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSubmitting, setIsSubmitting] = React.useState(false);
    // The code is single-use and rate-limited: send exactly one automatic request, even though StrictMode runs effects twice.
    const hasAutoSubmitted = React.useRef(false);

    const pair = React.useCallback(
        async (pairingCode: string, name: string) => {
            setErrorKey(null);
            setIsSubmitting(true);

            const result = await devicesApi.pair.post({
                body: { code: pairingCode, deviceName: name.trim() || defaultDeviceName },
            });

            if (!result.error) {
                navigate(routes.practiceProfiles, { replace: true });
                return;
            }

            setIsSubmitting(false);
            setErrorKey(
                result.error.status === 400
                    ? 'notificationPairingCodeInvalid'
                    : result.error.messageKey,
            );
        },
        [defaultDeviceName, navigate],
    );

    React.useEffect(() => {
        if (hasAutoSubmitted.current || !codePattern.test(code)) {
            return;
        }

        hasAutoSubmitted.current = true;
        void pair(code, defaultDeviceName);
        // Runs once on arrival with a code from the URL; later changes come from the form.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        void pair(code, deviceName);
    };

    return (
        <AuthCard
            title={getResource('captionPairDevice')}
            subtitle={getResource('captionPairDeviceDescription')}
            testId={testIds.practice.pairDevice}
        >
            <form onSubmit={handleSubmit} noValidate>
                <Stack spacing={2}>
                    {notice && !errorKey && <Alert severity="info">{getResource(notice)}</Alert>}
                    {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
                    <FormPinField
                        label={getResource('labelPairingCode')}
                        name="pairingCode"
                        length={codeLength}
                        masked={false}
                        autoFocus
                        required
                        disabled={isSubmitting}
                        value={code}
                        onChange={(value) => {
                            setErrorKey(null);
                            setCode(value);
                        }}
                    />
                    <FormTextField
                        label={getResource('labelDeviceName')}
                        name="deviceName"
                        disabled={isSubmitting}
                        value={deviceName}
                        onChange={setDeviceName}
                    />
                    <FormButton
                        label={getResource('labelPairDevice')}
                        type="submit"
                        disabled={!codePattern.test(code) || isSubmitting}
                    />
                </Stack>
            </form>
        </AuthCard>
    );
};

export default PairDevicePage;
