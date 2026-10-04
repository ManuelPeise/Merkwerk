import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    IDeleteGroupRequest,
    IGroup,
    IGroupInput,
    IUpdateGroupRequest,
} from 'src/lib/api/groups/groupsTypes';

/** Endpoints of GroupsController: everyone in the family reads, admins change (LP-108). */
export const groupsApi = {
    list: statelessApi.create<IGroup[]>({ serviceUrl: '/groups/list' }),
    create: statelessApi.create<IGroup, IGroupInput>({ serviceUrl: '/groups/create' }),
    update: statelessApi.create<IGroup, IUpdateGroupRequest>({ serviceUrl: '/groups/update' }),
    delete: statelessApi.create<void, IDeleteGroupRequest>({ serviceUrl: '/groups/delete' }),
};
