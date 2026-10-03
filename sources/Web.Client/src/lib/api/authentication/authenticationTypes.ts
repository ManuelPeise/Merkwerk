/** Mirrors the C# DTOs of AuthenticationController (Web.Core). */
export interface ILoginRequest {
    email: string;
    password: string;
}

export interface ISession {
    name: string;
    role: string;
    /** ISO date-time. */
    accessTokenExpiresAt: string;
}

export interface ICurrentUser {
    name: string;
    role: string;
}
