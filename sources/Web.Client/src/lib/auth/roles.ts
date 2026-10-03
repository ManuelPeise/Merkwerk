/** Role names as issued by the backend (LP-107). Components never use role strings directly. */
export const roles = {
    member: 'Member',
    orgAdmin: 'OrgAdmin',
    learner: 'Learner',
} as const;

export const adultRoles: string[] = [roles.member, roles.orgAdmin];
