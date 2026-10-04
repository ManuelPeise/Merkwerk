import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    IDeleteLearnerRequest,
    ILearner,
    ILearnerInput,
    IUpdateLearnerRequest,
} from 'src/lib/api/learners/learnersTypes';

/** Endpoints of LearnersController: everyone in the family reads, admins change (LP-105). */
export const learnersApi = {
    list: statelessApi.create<ILearner[]>({ serviceUrl: '/learners/list' }),
    create: statelessApi.create<ILearner, ILearnerInput>({ serviceUrl: '/learners/create' }),
    update: statelessApi.create<ILearner, IUpdateLearnerRequest>({
        serviceUrl: '/learners/update',
    }),
    delete: statelessApi.create<void, IDeleteLearnerRequest>({ serviceUrl: '/learners/delete' }),
};
