import React from 'react';
import { authenticationApi } from 'src/lib/api/authentication/authenticationApi';
import type { ICurrentUser, ILoginRequest } from 'src/lib/api/authentication/authenticationTypes';
import { onSessionExpired } from 'src/lib/api/apiClient';
import { AuthContext, type AuthStatus, type IAuthContext } from 'src/lib/auth/authContext';

interface IProps {
    children: React.ReactNode;
}

interface IAuthState {
    status: AuthStatus;
    user: ICurrentUser | null;
}

const anonymous: IAuthState = { status: 'anonymous', user: null };

/** Holds who is signed in. The tokens stay in HttpOnly cookies – the client only knows name and role. */
const AuthProvider: React.FC<IProps> = (props) => {
    const { children } = props;

    const [authState, setAuthState] = React.useState<IAuthState>({ status: 'loading', user: null });

    // After a reload the client knows nothing (HttpOnly cookies), so ask the server who is signed in.
    React.useEffect(() => {
        const controller = new AbortController();

        void authenticationApi.me.get({ signal: controller.signal }).then((result) => {
            if (result.error?.kind === 'canceled') {
                return;
            }

            setAuthState(result.data ? { status: 'authenticated', user: result.data } : anonymous);
        });

        return () => controller.abort();
    }, []);

    // A failed token refresh anywhere in the app ends the session; the route guards redirect to the login.
    React.useEffect(() => onSessionExpired(() => setAuthState(anonymous)), []);

    const login = React.useCallback(async (request: ILoginRequest) => {
        const result = await authenticationApi.login.post({ body: request });

        if (!result.data) {
            return result.error;
        }

        setAuthState({
            status: 'authenticated',
            user: { name: result.data.name, role: result.data.role },
        });

        return undefined;
    }, []);

    const logout = React.useCallback(async () => {
        await authenticationApi.logout.post();
        setAuthState(anonymous);
    }, []);

    const value = React.useMemo<IAuthContext>(
        () => ({ ...authState, login, logout }),
        [authState, login, logout],
    );

    return <AuthContext value={value}>{children}</AuthContext>;
};

export default AuthProvider;
