import React from 'react';
import { Alert, Box, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import ProfileTile from 'src/components/kids/ProfileTile';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { useTranslation } from 'src/hooks/useTranslation';
import { devicesApi } from 'src/lib/api/devices/devicesApi';
import { uiTestId, uiTestIdOf } from 'src/lib/testing/uiTestId';
import type { IDeviceProfile } from 'src/lib/api/devices/devicesTypes';
import type { NotificationKey } from 'src/lib/translations/translationKeys';
import { routes, type IPairState } from 'src/navigation/routes';

/** /practice/profiles - the child taps their picture; no typing needed. */
const ProfilesPage: React.FC = () => {
    const { getResource } = useTranslation();
    const { signInLearner } = useAuthentication();
    const navigate = useNavigate();

    const [profiles, setProfiles] = React.useState<IDeviceProfile[] | null>(null);
    const [errorKey, setErrorKey] = React.useState<NotificationKey | null>(null);
    const [isSigningIn, setIsSigningIn] = React.useState(false);

    React.useEffect(() => {
        const controller = new AbortController();

        void devicesApi.profiles.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            if (result.data) {
                setProfiles(result.data);
                return;
            }

            // The device cookie is gone or revoked: pair again.
            if (result.error.status === 401 || result.error.status === 403) {
                const state: IPairState = { notice: 'notificationDeviceNotPaired' };
                navigate(routes.practicePair, { replace: true, state });
                return;
            }

            setProfiles([]);
            setErrorKey(result.error.messageKey);
        });

        return () => controller.abort();
    }, [navigate]);

    const handleSelect = async (learnerId: number) => {
        setErrorKey(null);
        setIsSigningIn(true);

        const error = await signInLearner(learnerId);

        if (error) {
            setIsSigningIn(false);
            setErrorKey(error.messageKey);
            return;
        }

        navigate(routes.practice, { replace: true });
    };

    if (profiles === null) {
        return <LoadingIndicator uiTestId={uiTestId('loading-practice-profiles-page')} />;
    }

    return (
        <Stack spacing={3} data-testid={uiTestId('practice-profiles-page')}>
            <Typography variant="h1">{getResource('captionWhoIsPracticing')}</Typography>
            {errorKey && <Alert severity="error">{getResource(errorKey)}</Alert>}
            {profiles.length === 0 && !errorKey && (
                <Alert severity="info">{getResource('captionNoProfiles')}</Alert>
            )}
            <Box
                sx={{
                    display: 'grid',
                    gap: 2,
                    gridTemplateColumns: {
                        // minmax(0, ...): columns may shrink below the tile's content width on 360 px phones.
                        xs: 'repeat(2, minmax(0, 1fr))',
                        sm: 'repeat(3, minmax(0, 1fr))',
                        md: 'repeat(4, minmax(0, 1fr))',
                    },
                }}
            >
                {profiles.map((profile) => (
                    <ProfileTile
                        key={profile.learnerId}
                        name={profile.firstName}
                        avatarId={profile.avatarId}
                        disabled={isSigningIn}
                        uiTestId={uiTestIdOf(
                            uiTestId('practice-profile-tile'),
                            String(profile.learnerId),
                        )}
                        onSelect={() => void handleSelect(profile.learnerId)}
                    />
                ))}
            </Box>
        </Stack>
    );
};

export default ProfilesPage;
