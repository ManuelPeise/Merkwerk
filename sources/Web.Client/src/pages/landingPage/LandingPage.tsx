import React from 'react';
import { Box, Stack, Typography } from '@mui/material';
import { Navigate } from 'react-router-dom';
import { AdultIcon, ChildIcon } from 'src/components/icons/AppIcons';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import ChoiceTile from 'src/components/input/ChoiceTile';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useTranslation } from 'src/hooks/useTranslation';
import { adultRoles, roles } from 'src/lib/auth/roles';
import { routes } from 'src/navigation/routes';

/** Shared entry for children and adults: signed-in users are sent on, everyone else chooses. */
const LandingPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { status, user } = useAuthentication();

    if (status === 'loading') {
        return <LoadingIndicator />;
    }

    if (user && adultRoles.includes(user.role)) {
        return <Navigate to={routes.admin} replace />;
    }

    if (user?.role === roles.learner) {
        return <Navigate to={routes.practice} replace />;
    }

    return (
        <Stack
            spacing={{ xs: 5, md: 8 }}
            sx={{
                flexGrow: 1,
                alignItems: 'center',
                justifyContent: 'center',
                textAlign: 'center',
            }}
        >
            <Stack spacing={1}>
                <Typography variant="h1">{getResource('captionWelcome')}</Typography>
                <Typography variant="h5" component="p" color="text.secondary">
                    {getResource('captionWhoAreYou')}
                </Typography>
            </Stack>
            <Box
                sx={{
                    display: 'grid',
                    gap: { xs: 3, md: 6 },
                    width: '100%',
                    gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' },
                }}
            >
                <ChoiceTile
                    icon={<ChildIcon />}
                    label={getResource('labelPracticeEntry')}
                    description={getResource('labelPracticeEntryDescription')}
                    to={routes.practice}
                />
                <ChoiceTile
                    icon={<AdultIcon />}
                    label={getResource('labelAdultEntry')}
                    description={getResource('labelAdultEntryDescription')}
                    to={routes.login}
                    color="secondary"
                />
            </Box>
        </Stack>
    );
};

export default LandingPage;
