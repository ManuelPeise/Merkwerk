import React from 'react';
import { AppBar, Box, IconButton, Toolbar, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { LogoutIcon, MenuIcon } from 'src/components/icons/AppIcons';
import LanguageSwitch from 'src/components/layout/components/LanguageSwitch';
import { headerHeight } from 'src/components/layout/layoutConstants';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { testIdOf } from 'src/lib/testing/testIds';

interface IProps {
    /** Shown on small screens, where the drawer is hidden until opened. */
    showMenuButton: boolean;
    onMenuClick: () => void;
    testId?: string;
}

const HeaderBar: React.FC<IProps> = (props) => {
    const { showMenuButton, onMenuClick, testId } = props;

    const { getResource } = useTranslation();
    const { isAuthenticated, logout } = useAuthentication();

    return (
        <AppBar
            position="fixed"
            color="inherit"
            elevation={0}
            data-testid={testId}
            sx={{
                borderBottom: 1,
                borderColor: 'divider',
                // Above the drawer, so the header stays visible and the menu button stays reachable.
                zIndex: (theme) => theme.zIndex.drawer + 1,
            }}
        >
            <Toolbar sx={{ minHeight: headerHeight, gap: 2 }}>
                {showMenuButton && (
                    <IconButton
                        edge="start"
                        size="large"
                        aria-label={getResource('labelOpenMenu')}
                        onClick={onMenuClick}
                        sx={{ width: 56, height: 56 }}
                    >
                        <MenuIcon />
                    </IconButton>
                )}
                <Typography variant="h6" component="span" sx={{ fontWeight: 700 }}>
                    {getResource('captionAppTitle')}
                </Typography>
                <Box sx={{ ml: 'auto', display: 'flex', alignItems: 'center', gap: 1 }}>
                    <LanguageSwitch testId={testIdOf(testId, 'language-switch')} />
                    {isAuthenticated && (
                        <IconButton
                            size="large"
                            aria-label={getResource('labelLogout')}
                            onClick={() => void logout()}
                            sx={{ width: 56, height: 56 }}
                        >
                            <LogoutIcon />
                        </IconButton>
                    )}
                </Box>
            </Toolbar>
        </AppBar>
    );
};

export default HeaderBar;
