import { statelessApi } from 'src/lib/api/StatelessApi';
import type { IMember, IRemoveMemberRequest } from 'src/lib/api/members/membersTypes';

/** Endpoints of MembersController (LP-105). */
export const membersApi = {
    list: statelessApi.create<IMember[]>({ serviceUrl: '/members/list' }),
    remove: statelessApi.create<void, IRemoveMemberRequest>({ serviceUrl: '/members/remove' }),
};
