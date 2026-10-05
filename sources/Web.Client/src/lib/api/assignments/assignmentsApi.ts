import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    IAssignExerciseRequest,
    IAssignment,
    IRevokeAssignmentRequest,
} from 'src/lib/api/assignments/assignmentsTypes';

/** Endpoints of AssignmentsController: every adult of the family assigns and revokes (LP-114). */
export const assignmentsApi = {
    /** params: { exerciseId } */
    list: statelessApi.create<IAssignment[]>({ serviceUrl: '/assignments/list' }),
    assign: statelessApi.create<IAssignment[], IAssignExerciseRequest>({
        serviceUrl: '/assignments/assign',
    }),
    revoke: statelessApi.create<void, IRevokeAssignmentRequest>({
        serviceUrl: '/assignments/revoke',
    }),
};
