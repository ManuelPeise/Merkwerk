/** Mirrors the C# DTOs of InvitationsController (Web.Core). */
export interface IInvitationDetails {
    familyName: string;
    email: string;
    invitedBy: string;
    /** ISO date-time. */
    expiresAt: string;
}

export interface IInvitationAcceptRequest {
    token: string;
    displayName: string;
    password: string;
    privacyAccepted: boolean;
}
