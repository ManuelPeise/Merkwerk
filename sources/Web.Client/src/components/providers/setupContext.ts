import { createContext } from 'react';

export interface ISetupContext {
    /** Called by the setup page after the instance was initialised, so the SetupGate stops redirecting. */
    markSetupDone: () => void;
}

// Own file: react-refresh only works when component files export nothing but components.
export const SetupContext = createContext<ISetupContext | undefined>(undefined);
