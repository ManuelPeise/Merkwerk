import React from 'react';
import { Stack, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { useAuthentication } from 'src/hooks/useAuthentication';

/** Placeholder for the parents' area - only reachable when signed in (ProtectedRoute). */
const AdminPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { user } = useAuthentication();

    return (
        <Stack spacing={2}>
            <Typography variant="h1">{getResource('captionAdminArea')}</Typography>
            <Typography color="text.secondary">{user?.name}</Typography>
        </Stack>
    );
};

export default AdminPage;
