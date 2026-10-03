import React from 'react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';
import LoadingIndicator from 'src/components/feedback/LoadingIndicator';
import { SetupContext, type ISetupContext } from 'src/components/providers/setupContext';
import { useSetupStatus } from 'src/hooks/useSetupStatus';
import { routes } from 'src/navigation/routes';

/**
 * Outermost route element. While the instance isn't set up, every route leads to /setup;
 * once it is, /setup leads back to the start page.
 */
const SetupGate: React.FC = () => {
    const { status, isSetupRequired, markSetupDone } = useSetupStatus();
    const { pathname } = useLocation();

    const context = React.useMemo<ISetupContext>(() => ({ markSetupDone }), [markSetupDone]);

    if (status === 'loading') {
        return <LoadingIndicator />;
    }

    if (isSetupRequired && pathname !== routes.setup) {
        return <Navigate to={routes.setup} replace />;
    }

    if (!isSetupRequired && pathname === routes.setup) {
        return <Navigate to={routes.start} replace />;
    }

    return (
        <SetupContext.Provider value={context}>
            <Outlet />
        </SetupContext.Provider>
    );
};

export default SetupGate;
