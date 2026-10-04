export type TestId = string;

export const testIdOf = (base: string | undefined, part: string): string | undefined =>
    base ? `${base}.${part}` : undefined;

export const testIds = {
    layout: {
        public: 'layout.public',
        admin: 'layout.admin',
        kids: 'layout.kids',
        header: 'layout.header',
        drawer: 'layout.drawer',
        languageSwitch: 'layout.language-switch',
        authCard: 'layout.auth-card',
        drawerItem: (routeName: string) => `layout.drawer.item-${routeName}`,
    },
    auth: {
        login: 'auth.login',
        forgotPassword: 'auth.forgot-password',
        resetPassword: 'auth.reset-password',
        setup: 'auth.setup',
        invitation: 'auth.invitation',
        confirmEmail: 'auth.confirm-email',
    },
    family: {
        page: 'family.page',
        learners: 'family.learners',
        learnerItem: (id: number) => `family.learners.item.${id}`,
        learnerDialog: 'family.learner-dialog',
        groups: 'family.groups',
        groupItem: (id: number) => `family.groups.item.${id}`,
        groupDialog: 'family.group-dialog',
        adults: 'family.adults',
        memberItem: (membershipId: number) => `family.adults.item.${membershipId}`,
        invitationItem: (id: number) => `family.invitations.item.${id}`,
    },
    devices: {
        page: 'devices.page',
        pairingCode: 'devices.pairing-code',
        pairedList: 'devices.paired-list',
        deviceItem: (id: number) => `devices.paired-list.item.${id}`,
    },
    subjects: {
        page: 'subjects.page',
        list: 'subjects.list',
        item: (id: number) => `subjects.list.item.${id}`,
        dialog: 'subjects.dialog',
    },
    practice: {
        pairDevice: 'practice.pair-device',
        profiles: 'practice.profiles',
        profileTile: (learnerId: number) => `practice.profiles.tile.${learnerId}`,
        home: 'practice.home',
    },
} as const;
