import React from 'react';
import { Box, Container, Toolbar, useMediaQuery, useTheme } from '@mui/material';
import { Outlet } from 'react-router-dom';
import HeaderBar from 'src/components/layout/HeaderBar';
import { headerHeight } from 'src/components/layout/layoutConstants';
import NavigationDrawer from 'src/components/layout/NavigationDrawer';
import { uiTestId } from 'src/lib/testing/uiTestId';

/** Shell for the parents' area: header bar on top, navigation drawer on the left, page content via <Outlet />. */
const AdminLayout: React.FC = () => {
    const theme = useTheme();
    // noSsr: evaluate immediately, so large screens don't flash the temporary drawer first.
    const isDesktop = useMediaQuery(theme.breakpoints.up('md'), { noSsr: true });
    const [isDrawerOpen, setIsDrawerOpen] = React.useState(false);

    return (
        <Box sx={{ display: 'flex', minHeight: '100dvh' }} data-testid={uiTestId('layout-admin')}>
            <HeaderBar
                showMenuButton={!isDesktop}
                onMenuClick={() => setIsDrawerOpen((open) => !open)}
                uiTestId={uiTestId('layout-header')}
            />
            <NavigationDrawer
                variant={isDesktop ? 'permanent' : 'temporary'}
                open={isDrawerOpen}
                onClose={() => setIsDrawerOpen(false)}
                uiTestId={uiTestId('layout-drawer')}
            />
            <Box component="main" sx={{ flexGrow: 1, minWidth: 0 }}>
                {/* Spacer below the fixed header bar. */}
                <Toolbar sx={{ minHeight: headerHeight }} />
                <Container maxWidth="lg" sx={{ py: 4 }}>
                    <Outlet />
                </Container>
            </Box>
        </Box>
    );
};

export default AdminLayout;
