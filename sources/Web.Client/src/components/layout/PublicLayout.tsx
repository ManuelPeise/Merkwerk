import React from 'react';
import { AppBar, Box, Container, Link as MuiLink, Toolbar, Typography } from '@mui/material';
import { Link, Outlet } from 'react-router-dom';
import LanguageSwitch from 'src/components/layout/components/LanguageSwitch';
import { headerHeight } from 'src/components/layout/layoutConstants';
import { uiTestId } from 'src/lib/testing/uiTestId';
import { useTranslation } from 'src/hooks/useTranslation';
import { routes } from 'src/navigation/routes';

const PublicLayout: React.FC = () => {
    const { getResource } = useTranslation();

    return (
        <Box
            sx={{ minHeight: '100dvh', display: 'flex', flexDirection: 'column' }}
            data-testid={uiTestId('layout-public')}
        >
            <AppBar
                position="static"
                color="inherit"
                elevation={0}
                sx={{ borderBottom: 1, borderColor: 'divider' }}
            >
                <Toolbar sx={{ minHeight: headerHeight, gap: 2 }}>
                    <MuiLink
                        component={Link}
                        to={routes.start}
                        underline="none"
                        color="inherit"
                        sx={{ display: 'flex', alignItems: 'center', gap: 1, minHeight: 48 }}
                    >
                        <Box
                            component="img"
                            src="/favicon.svg"
                            alt=""
                            sx={{ width: 40, height: 40 }}
                        />
                        <Typography variant="h6" component="span" sx={{ fontWeight: 700 }}>
                            {getResource('captionAppTitle')}
                        </Typography>
                    </MuiLink>
                    <Box sx={{ ml: 'auto' }}>
                        <LanguageSwitch uiTestId={uiTestId('layout-language-switch')} />
                    </Box>
                </Toolbar>
            </AppBar>
            <Container
                component="main"
                maxWidth="xl"
                sx={{ py: { xs: 4, md: 8 }, flexGrow: 1, display: 'flex', flexDirection: 'column' }}
            >
                <Outlet />
            </Container>
        </Box>
    );
};

export default PublicLayout;
