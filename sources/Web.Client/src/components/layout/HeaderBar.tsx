import React from 'react';
import { AppBar, IconButton, Toolbar, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { LogoutIcon, MenuIcon } from 'src/components/icons/AppIcons';
import { headerHeight } from 'src/components/layout/layoutConstants';
import { useAuthentication } from 'src/hooks/useAuthentication';

interface IProps {
    /** Shown on small screens, where the drawer is hidden until opened. */
    showMenuButton: boolean;
    onMenuClick: () => void;
}

const HeaderBar: React.FC<IProps> = (props) => {
    const { showMenuButton, onMenuClick } = props;

    const { getResource } = useTranslation();
    const { isAuthenticated, logout } = useAuthentication();

    return (
        <AppBar
            position="fixed"
            color="inherit"
            elevation={0}
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
                {isAuthenticated && (
                    <IconButton
                        size="large"
                        aria-label={getResource('labelLogout')}
                        onClick={() => void logout()}
                        sx={{ ml: 'auto', width: 56, height: 56 }}
                    >
                        <LogoutIcon />
                    </IconButton>
                )}
            </Toolbar>
        </AppBar>
    );
};

export default HeaderBar;
