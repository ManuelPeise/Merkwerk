import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { uiTestId } from 'src/lib/testing/uiTestId';
import { routes, type IRedirectState } from 'src/navigation/routes';

/**
 * Child routes only for users who are NOT signed in (e.g. login). Signed-in users are sent on -
 * back to where the ProtectedRoute came from, or to the start page.
 */
const PublicRoute: React.FC = () => {
    const { status } = useAuthentication();
    const location = useLocation();

    if (status === 'loading') {
        return <LoadingIndicator uiTestId={uiTestId('loading-public-route')} />;
    }

    if (status === 'authenticated') {
        const state = location.state as IRedirectState | null;
        return <Navigate to={state?.from ?? routes.start} replace />;
    }

    return <Outlet />;
};

export default PublicRoute;
