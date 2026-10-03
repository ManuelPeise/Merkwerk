export const routes = {
    start: '/',
    login: '/login',
    admin: '/admin',
} as const;

/** Location state the guards pass to the login, so it can send the user back afterwards. */
export interface IRedirectState {
    from?: string;
}
