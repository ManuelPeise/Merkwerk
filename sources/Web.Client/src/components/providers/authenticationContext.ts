import { createContext } from 'react';
import type { ICurrentUser, ILoginRequest } from 'src/lib/api/authentication/authenticationTypes';
import type { ApiError } from 'src/lib/api/types/apiError';

/** loading = asking the server after a (re)load; the route guards wait until it is decided. */
export type AuthenticationStatus = 'loading' | 'authenticated' | 'anonymous';

export interface IAuthenticationContext {
    status: AuthenticationStatus;
    /** Shortcut for status === 'authenticated'. */
    isAuthenticated: boolean;
    user: ICurrentUser | null;
    /** Returns the error, or undefined on success. */
    login: (request: ILoginRequest) => Promise<ApiError | undefined>;
    logout: () => Promise<void>;
}

// Own file: react-refresh (Fast Refresh) only works when component files export nothing but components.
export const AuthenticationContext = createContext<IAuthenticationContext | undefined>(undefined);
