import React from 'react';
import {
    Box,
    Drawer,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Toolbar,
} from '@mui/material';
import { Link, matchPath, useLocation } from 'react-router-dom';
import { useTranslation } from 'src/hooks/useTranslation';
import { drawerWidth, headerHeight } from 'src/components/layout/layoutConstants';
import { navigationItems } from 'src/navigation/navigationItems';
import { routes } from 'src/navigation/routes';

interface IProps {
    /** `permanent` on large screens, `temporary` (opened from the header) on phones and tablets. */
    variant: 'permanent' | 'temporary';
    open: boolean;
    onClose: () => void;
}

const NavigationDrawer: React.FC<IProps> = (props) => {
    const { variant, open, onClose } = props;

    const { getResource } = useTranslation();
    const { pathname } = useLocation();

    const isActive = (path: string): boolean =>
        matchPath({ path, end: path === routes.start }, pathname) !== null;

    return (
        <Drawer
            variant={variant}
            open={variant === 'permanent' || open}
            onClose={onClose}
            sx={{
                width: drawerWidth,
                flexShrink: 0,
                '& .MuiDrawer-paper': { width: drawerWidth, boxSizing: 'border-box' },
            }}
        >
            {/* Spacer below the fixed header bar. */}
            <Toolbar sx={{ minHeight: headerHeight }} />
            <Box component="nav" aria-label={getResource('labelMainNavigation')}>
                <List>
                    {navigationItems.map(({ path, labelKey, icon: Icon }) => (
                        <ListItemButton
                            key={path}
                            component={Link}
                            to={path}
                            selected={isActive(path)}
                            onClick={onClose}
                            sx={{ minHeight: 64, mx: 1, borderRadius: 2 }}
                        >
                            <ListItemIcon>
                                <Icon />
                            </ListItemIcon>
                            <ListItemText primary={getResource(labelKey)} />
                        </ListItemButton>
                    ))}
                </List>
            </Box>
        </Drawer>
    );
};

export default NavigationDrawer;
