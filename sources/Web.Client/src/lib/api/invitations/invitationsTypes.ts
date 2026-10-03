/** Mirrors the C# DTOs of InvitationsController (Web.Core). */
export interface IInvitationDetails {
    familyName: string;
    email: string;
    invitedBy: string;
    /** ISO date-time. */
    expiresAt: string;
}

/**
 * New account: displayName, password and privacyAccepted are required.
 * Already signed in (existing account): only the token – the server adds the membership to the current user.
 */
export interface IInvitationAcceptRequest {
    token: string;
    displayName?: string;
    password?: string;
    privacyAccepted?: boolean;
}
