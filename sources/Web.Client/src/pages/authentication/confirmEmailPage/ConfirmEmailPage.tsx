import React from 'react';
import { Alert, Button, Stack } from '@mui/material';
import { Link, useSearchParams } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import AuthCard from 'src/components/layout/AuthCard';
import { useTranslation } from 'src/hooks/useTranslation';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import { routes } from 'src/navigation/routes';

type ConfirmState = 'loading' | 'success' | 'failed';

/** /confirm-email?userId=…&token=… – confirms the address as soon as the page opens. */
const ConfirmEmailPage: React.FC = () => {
    const { getResource } = useTranslation();
    const [searchParams] = useSearchParams();
    const userId = searchParams.get('userId');
    const token = searchParams.get('token');

    const [confirmState, setConfirmState] = React.useState<ConfirmState>(
        userId === null || token === null ? 'failed' : 'loading',
    );
    // The token works once: send exactly one request, even though StrictMode runs effects twice in development.
    const hasStarted = React.useRef(false);

    React.useEffect(() => {
        if (userId === null || token === null || hasStarted.current) {
            return;
        }

        hasStarted.current = true;

        // No AbortSignal on purpose: aborting the only request would leave the page loading forever.
        void authenticationApi.confirmEmail.post({ body: { userId, token } }).then((result) => {
            setConfirmState(result.error ? 'failed' : 'success');
        });
    }, [userId, token]);

    return (
        <AuthCard title={getResource('captionConfirmEmail')}>
            {confirmState === 'loading' ? (
                <LoadingIndicator />
            ) : (
                <Stack spacing={2}>
                    <Alert severity={confirmState === 'success' ? 'success' : 'error'}>
                        {getResource(
                            confirmState === 'success'
                                ? 'notificationEmailConfirmed'
                                : 'notificationLinkExpired',
                        )}
                    </Alert>
                    <Button component={Link} to={routes.login} variant="contained">
                        {getResource('labelToLogin')}
                    </Button>
                </Stack>
            )}
        </AuthCard>
    );
};

export default ConfirmEmailPage;
