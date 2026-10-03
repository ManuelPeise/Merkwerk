import React, { useState, type PropsWithChildren } from 'react';
import {
    AuthenticationContext,
    type AuthenticationStatus,
    type IAuthenticationContext,
} from 'src/components/providers/authenticationContext';
import { onSessionExpired } from 'src/lib/api/apiClient';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import type { ICurrentUser, ILoginRequest } from 'src/lib/api/authentication/authenticationTypes';

type IAuthenticationProviderProps = PropsWithChildren;

interface IAuthenticationState {
    status: AuthenticationStatus;
    user: ICurrentUser | null;
}

const anonymous: IAuthenticationState = { status: 'anonymous', user: null };

/** Holds who is signed in. The tokens stay in HttpOnly cookies – the client only knows name and role. */
const AuthenticationProvider: React.FC<IAuthenticationProviderProps> = (props) => {
    const { children } = props;

    const [authenticationState, setAuthenticationState] = useState<IAuthenticationState>({
        status: 'loading',
        user: null,
    });

    // After a reload the client knows nothing (HttpOnly cookies), so ask the server who is signed in.
    React.useEffect(() => {
        const controller = new AbortController();

        void authenticationApi.me.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setAuthenticationState(
                result.data ? { status: 'authenticated', user: result.data } : anonymous,
            );
        });

        return () => controller.abort();
    }, []);

    // A failed token refresh anywhere in the app ends the session; the route guards redirect to the login.
    React.useEffect(() => onSessionExpired(() => setAuthenticationState(anonymous)), []);

    const login = React.useCallback(async (request: ILoginRequest) => {
        const result = await authenticationApi.login.post({ body: request });

        if (!result.data) {
            return result.error;
        }

        setAuthenticationState({
            status: 'authenticated',
            user: { name: result.data.name, role: result.data.role },
        });

        return undefined;
    }, []);

    const logout = React.useCallback(async () => {
        await authenticationApi.logout.post();
        setAuthenticationState(anonymous);
    }, []);

    // Memoized, so consumers only re-render when the authentication state really changes.
    const value = React.useMemo<IAuthenticationContext>(
        () => ({
            ...authenticationState,
            isAuthenticated: authenticationState.status === 'authenticated',
            login,
            logout,
        }),
        [authenticationState, login, logout],
    );

    return (
        <AuthenticationContext.Provider value={value}>{children}</AuthenticationContext.Provider>
    );
};

export default AuthenticationProvider;
