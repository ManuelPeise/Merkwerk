import { statelessApi } from 'src/lib/api/StatelessApi';
import type {
    IArchiveExerciseRequest,
    IExerciseDetail,
    IExerciseIdRequest,
    IExerciseSummary,
    IGeneratePreviewRequest,
    IGeneratorPreview,
    ISaveExerciseRequest,
    IUpdateExerciseRequest,
} from 'src/lib/api/exercises/exercisesTypes';

/** Endpoints of ExercisesController: every adult of the family reads and changes (LP-110, LP-131). */
export const exercisesApi = {
    list: statelessApi.create<IExerciseSummary[]>({ serviceUrl: '/exercises/list' }),
    /** params: { id } */
    get: statelessApi.create<IExerciseDetail>({ serviceUrl: '/exercises/get' }),
    create: statelessApi.create<IExerciseSummary, ISaveExerciseRequest>({
        serviceUrl: '/exercises/create',
    }),
    update: statelessApi.create<IExerciseSummary, IUpdateExerciseRequest>({
        serviceUrl: '/exercises/update',
    }),
    publish: statelessApi.create<IExerciseSummary, IExerciseIdRequest>({
        serviceUrl: '/exercises/publish',
    }),
    archive: statelessApi.create<IExerciseSummary, IArchiveExerciseRequest>({
        serviceUrl: '/exercises/archive',
    }),
    generatePreview: statelessApi.create<IGeneratorPreview, IGeneratePreviewRequest>({
        serviceUrl: '/exercises/generate-preview',
    }),
};
