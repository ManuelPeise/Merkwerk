import React from 'react';
import { AppBar, Box, Button, Container, Toolbar, Typography } from '@mui/material';
import { Outlet, useNavigate } from 'react-router-dom';
import AvatarImage from 'src/components/kids/AvatarImage';
import { SwitchAccountIcon } from 'src/components/icons/AppIcons';
import { kidsHeaderHeight } from 'src/components/layout/layoutConstants';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { roles } from 'src/lib/auth/roles';
import { useTranslation } from 'src/hooks/useTranslation';
import { routes } from 'src/navigation/routes';

/** Shell for the children's area: large header with the child and "switch child", no menu, no language choice. */
const KidsLayout: React.FC = () => {
    const { getResource } = useTranslation();
    const { user, logout } = useAuthentication();

    const navigate = useNavigate();

    // An adult signed in on the child's device is not shown here ("switch child" would sign them out).
    const learner = user?.role === roles.learner ? user : null;
    const name = learner?.name ?? '';

    const handleSwitchChild = async () => {
        await logout();
        navigate(routes.practiceProfiles);
    };

    return (
        <Box sx={{ minHeight: '100dvh', display: 'flex', flexDirection: 'column' }}>
            <AppBar
                position="static"
                color="inherit"
                elevation={0}
                sx={{ borderBottom: 1, borderColor: 'divider' }}
            >
                <Toolbar sx={{ minHeight: kidsHeaderHeight, gap: 2 }}>
                    {/* Without a child (profile selection) the header stays empty. */}
                    {learner && (
                        <>
                            <AvatarImage avatarId={learner.avatarId} name={name} size={56} />
                            <Typography
                                variant="h6"
                                component="span"
                                noWrap
                                sx={{ fontWeight: 700 }}
                            >
                                {name}
                            </Typography>
                            <Button
                                variant="outlined"
                                startIcon={<SwitchAccountIcon />}
                                onClick={handleSwitchChild}
                                sx={{ ml: 'auto', minWidth: 64, minHeight: 64, flexShrink: 0 }}
                            >
                                {getResource('labelSwitchChild')}
                            </Button>
                        </>
                    )}
                </Toolbar>
            </AppBar>
            <Container component="main" maxWidth="md" sx={{ py: 4, flexGrow: 1 }}>
                <Outlet />
            </Container>
        </Box>
    );
};

export default KidsLayout;
