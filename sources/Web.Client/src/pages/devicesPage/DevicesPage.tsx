import React from 'react';
import { Stack, Typography } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';
import { testIds } from 'src/lib/testing/testIds';
import DevicesSection from 'src/pages/devicesPage/components/DevicesSection';
import PairingCodeSection from 'src/pages/devicesPage/components/PairingCodeSection';

/** /admin/devices – pair tablets and phones with the family and unpair them (LP-106). */
const DevicesPage: React.FC = () => {
    const { getResource } = useTranslation();

    return (
        <Stack spacing={5} data-testid={testIds.devices.page}>
            <Stack spacing={1}>
                <Typography variant="h1">{getResource('captionDevices')}</Typography>
                <Typography color="text.secondary">
                    {getResource('captionDevicesDescription')}
                </Typography>
            </Stack>
            <PairingCodeSection />
            <DevicesSection />
        </Stack>
    );
};

export default DevicesPage;
