import { useContext } from 'react';
import { SetupContext, type ISetupContext } from 'src/components/providers/setupContext';

/** Lets the setup page tell the SetupGate that the setup is done. Only inside <SetupGate>. */
export const useSetupCompletion = (): ISetupContext => {
    const context = useContext(SetupContext);

    if (!context) {
        throw new Error('useSetupCompletion must be used within a SetupGate');
    }

    return context;
};
