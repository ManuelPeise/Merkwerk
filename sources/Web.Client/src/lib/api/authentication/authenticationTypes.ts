/** Mirrors the C# DTOs of AuthenticationController (Web.Core). */
export interface ILoginRequest {
    email: string;
    password: string;
}

export interface ISession {
    name: string;
    role: string;
    /** Start password active: only change-password works until it is replaced (LP-104). */
    mustChangePassword?: boolean;
    /** Only for learners. */
    avatarId?: string;
    /** ISO date-time. */
    accessTokenExpiresAt: string;
}

export interface ICurrentUser {
    name: string;
    role: string;
    mustChangePassword?: boolean;
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

export interface IChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
}

export interface IConfirmEmailRequest {
    userId: string;
    token: string;
}
