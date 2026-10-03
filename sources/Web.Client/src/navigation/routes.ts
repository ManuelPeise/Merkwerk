export const routes = {
    start: '/',
    login: '/login',
    forgotPassword: '/forgot-password',
    resetPassword: '/reset-password',
    setup: '/setup',
    invitation: '/invitation',
    confirmEmail: '/confirm-email',
    admin: '/admin',
    practice: '/practice',
    practicePair: '/practice/pair',
    practiceProfiles: '/practice/profiles',
} as const;

/** Location state the guards pass to the login, so it can send the user back afterwards. */
export interface IRedirectState {
    from?: string;
}
