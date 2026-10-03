/** Mirrors the C# DTOs of AuthenticationController (Web.Core). */
export interface ILoginRequest {
    email: string;
    password: string;
}

export interface ISession {
    name: string;
    role: string;
    /** Only for learners. */
    avatarId?: string;
    /** ISO date-time. */
    accessTokenExpiresAt: string;
}

export interface ICurrentUser {
    name: string;
    role: string;
    avatarId?: string;
}

export interface IForgotPasswordRequest {
    email: string;
}

export interface IResetPasswordRequest {
    email: string;
    token: string;
    newPassword: string;
}

export interface IConfirmEmailRequest {
    userId: string;
    token: string;
}
