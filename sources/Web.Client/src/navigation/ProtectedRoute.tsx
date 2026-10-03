import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { useAuthentication } from 'src/hooks/useAuthentication';
import { routes, type IRedirectState } from 'src/navigation/routes';

interface IProps {
    /** Only these roles may enter; omit for "any signed-in user". */
    roles?: string[];
    /** Where signed-out visitors go instead of the login (e.g. the profile selection). */
    signedOutTo?: string;
}

/** Child routes only for signed-in users. Others go to the login and come back afterwards. */
const ProtectedRoute: React.FC<IProps> = (props) => {
    const { roles, signedOutTo } = props;

    const { status, user } = useAuthentication();
    const location = useLocation();

    if (status === 'loading') {
        return <LoadingIndicator />;
    }

    if (!user) {
        if (signedOutTo) {
            return <Navigate to={signedOutTo} replace />;
        }

        const state: IRedirectState = { from: location.pathname };
        return <Navigate to={routes.login} replace state={state} />;
    }

    if (roles && !roles.includes(user.role)) {
        return <Navigate to={routes.start} replace />;
    }

    return <Outlet />;
};

export default ProtectedRoute;
