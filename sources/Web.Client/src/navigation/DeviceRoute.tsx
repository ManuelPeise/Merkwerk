import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { useDeviceStatus } from 'src/hooks/useDeviceStatus';
import { uiTestId } from 'src/lib/testing/uiTestId';
import { routes } from 'src/navigation/routes';

/** Child routes only for paired devices; others are sent to "pair device". */
const DeviceRoute: React.FC = () => {
    const { status, isPaired } = useDeviceStatus();

    if (status === 'loading') {
        return <LoadingIndicator uiTestId={uiTestId('loading-device-route')} />;
    }

    if (!isPaired) {
        return <Navigate to={routes.practicePair} replace />;
    }

    return <Outlet />;
};

export default DeviceRoute;
