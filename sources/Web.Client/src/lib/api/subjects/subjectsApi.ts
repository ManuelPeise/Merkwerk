import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    ISubject,
    ISubjectInput,
    IUpdateSubjectRequest,
} from 'src/lib/api/subjects/subjectsTypes';

/** Endpoints of SubjectsController: every adult reads, the family's admin creates and changes (LP-109). */
export const subjectsApi = {
    list: statelessApi.create<ISubject[]>({ serviceUrl: '/subjects/list' }),
    create: statelessApi.create<ISubject, ISubjectInput>({ serviceUrl: '/subjects/create' }),
    update: statelessApi.create<ISubject, IUpdateSubjectRequest>({
        serviceUrl: '/subjects/update',
    }),
};
