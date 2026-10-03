import React from 'react';
import { AuthContext, type IAuthContext } from 'src/lib/auth/authContext';

/** Who is signed in, plus login/logout. Only inside <AuthProvider> (see App.tsx). */
export const useAuth = (): IAuthContext => {
    const context = React.useContext(AuthContext);

    if (!context) {
        throw new Error('useAuth must be used inside <AuthProvider>.');
    }

    return context;
};
