import { useContext } from 'react';
import {
    AuthenticationContext,
    type IAuthenticationContext,
} from 'src/components/providers/authenticationContext';

/** Who is signed in, plus login/logout. Only inside <AuthenticationProvider> (see App.tsx). */
export const useAuthentication = (): IAuthenticationContext => {
    const context = useContext(AuthenticationContext);

    if (!context) {
        throw new Error('useAuthentication must be used within an AuthenticationProvider');
    }

    return context;
};
