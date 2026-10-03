/** Mirrors the C# DTOs of MembersController (Web.Core). */
export type OrganizationRole = 'Member' | 'OrgAdmin';

export interface IMember {
    membershipId: number;
    userId: number;
    displayName: string;
    email: string;
    role: OrganizationRole;
    isOwner: boolean;
}

export interface IRemoveMemberRequest {
    membershipId: number;
}
