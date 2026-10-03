import { createContext } from 'react';
import type { ICurrentUser, ILoginRequest } from 'src/lib/api/authentication/authenticationTypes';
import type { ApiError } from 'src/lib/api/types/apiError';

/** loading = asking the server after a (re)load; the route guards wait until it is decided. */
export type AuthStatus = 'loading' | 'authenticated' | 'anonymous';

export interface IAuthContext {
    status: AuthStatus;
    user: ICurrentUser | null;
    /** Returns the error, or undefined on success. */
    login: (request: ILoginRequest) => Promise<ApiError | undefined>;
    logout: () => Promise<void>;
}

export const AuthContext = createContext<IAuthContext | null>(null);
