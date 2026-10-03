import type { ComponentType } from 'react';
import type { SvgIconProps } from '@mui/material';
import { AdminIcon, HomeIcon, LoginIcon } from 'src/components/icons/AppIcons';
import type { AuthenticationStatus } from 'src/components/providers/authenticationContext';
import type { LabelKey } from 'src/lib/translations/translationKeys';
import { routes } from 'src/navigation/routes';

/** always = everyone, authenticated = signed in (ProtectedRoute), anonymous = not signed in (PublicRoute). */
export type NavigationVisibility = 'always' | 'authenticated' | 'anonymous';

export type NavigationItem = {
    path: string;
    labelKey: LabelKey;
    icon: ComponentType<SvgIconProps>;
    visibility: NavigationVisibility;
};

/** Entries of the navigation drawer, in display order. Keep visibility in line with the route guards. */
export const navigationItems: readonly NavigationItem[] = [
    { path: routes.start, labelKey: 'labelNavigationStart', icon: HomeIcon, visibility: 'always' },
    { path: routes.admin, labelKey: 'labelNavigationAdmin', icon: AdminIcon, visibility: 'authenticated' },
    { path: routes.login, labelKey: 'labelNavigationLogin', icon: LoginIcon, visibility: 'anonymous' },
];

export const isNavigationItemVisible = (item: NavigationItem, status: AuthenticationStatus): boolean =>
    item.visibility === 'always' ||
    (item.visibility === 'authenticated' && status === 'authenticated') ||
    (item.visibility === 'anonymous' && status === 'anonymous');
