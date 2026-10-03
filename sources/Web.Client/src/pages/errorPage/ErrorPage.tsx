import React from 'react';
import { Button, Container, Stack, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { routes } from 'src/navigation/routes';

/**
 * errorElement of the router: shown when a page throws while loading or rendering.
 * No technical details for the user; the buttons reload the app, which resets its state.
 */
const ErrorPage: React.FC = () => {
    const { getResource } = useTranslation();

    return (
        <Container maxWidth="sm" sx={{ py: 8 }}>
            <Stack spacing={3} sx={{ alignItems: 'flex-start' }}>
                <Typography variant="h1">{getResource('captionError')}</Typography>
                <Typography color="text.secondary">
                    {getResource('notificationUnexpectedError')}
                </Typography>
                <Stack direction="row" spacing={2}>
                    <Button variant="contained" onClick={() => window.location.reload()}>
                        {getResource('labelReload')}
                    </Button>
                    <Button variant="outlined" href={routes.start}>
                        {getResource('labelBackToStart')}
                    </Button>
                </Stack>
            </Stack>
        </Container>
    );
};

export default ErrorPage;
