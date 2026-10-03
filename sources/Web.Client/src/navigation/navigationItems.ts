import type { ComponentType } from 'react';
import type { SvgIconProps } from '@mui/material';
import { AdminIcon, DevicesIcon, FamilyIcon } from 'src/components/icons/AppIcons';
import type { LabelKey } from 'src/lib/translations/translationKeys';
import { routes } from 'src/navigation/routes';

export type NavigationItem = {
    path: string;
    labelKey: LabelKey;
    icon: ComponentType<SvgIconProps>;
};

/** Entries of the parents' navigation drawer, in display order. Public pages have no menu. */
export const navigationItems: readonly NavigationItem[] = [
    { path: routes.admin, labelKey: 'labelNavigationAdmin', icon: AdminIcon },
    { path: routes.family, labelKey: 'labelNavigationFamily', icon: FamilyIcon },
    { path: routes.devices, labelKey: 'labelNavigationDevices', icon: DevicesIcon },
];
