export const routes = {
    start: '/',
    login: '/login',
    forgotPassword: '/forgot-password',
    resetPassword: '/reset-password',
    setup: '/setup',
    invitation: '/invitation',
    confirmEmail: '/confirm-email',
    admin: '/admin',
    family: '/admin/family',
    devices: '/admin/devices',
    subjects: '/admin/subjects',
    practice: '/practice',
    practicePair: '/practice/pair',
    practiceProfiles: '/practice/profiles',
} as const;

/** Location state for the pairing page, e.g. when a paired device was revoked. */
export interface IPairState {
    notice?: 'notificationDeviceNotPaired';
}

/** Location state the guards pass to the login, so it can send the user back afterwards (path incl. query). */
export interface IRedirectState {
    from?: string;
}
