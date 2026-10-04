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
    exercises: '/admin/exercises',
    /** :id = exercise id or "new" - build links with toExerciseEditor. */
    exerciseEditor: '/admin/exercises/:id',
    practice: '/practice',
    practicePair: '/practice/pair',
    practiceProfiles: '/practice/profiles',
} as const;

/** Link to the exercise editor (LP-112); 'new' creates an exercise. */
export const toExerciseEditor = (id: number | 'new'): string => `${routes.exercises}/${id}`;

/** Location state for the pairing page, e.g. when a paired device was revoked. */
export interface IPairState {
    notice?: 'notificationDeviceNotPaired';
}

/** Location state the guards pass to the login, so it can send the user back afterwards (path incl. query). */
export interface IRedirectState {
    from?: string;
}
