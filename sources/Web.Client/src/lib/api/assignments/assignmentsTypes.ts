import type { GeneratorSettings } from 'src/lib/api/exercises/exercisesTypes';

/** Mirrors the C# DTOs of AssignmentsController (Web.Core, LP-114). */
export interface IAssignment {
    id: number;
    exerciseId: number;
    /** Exactly one of learnerId and groupId is set. */
    learnerId: number | null;
    groupId: number | null;
    /** Calendar day "yyyy-MM-dd" or null. */
    dueDate: string | null;
    allowDotArray: boolean;
    allowTimesTableMatrix: boolean;
    /** Settings of this assignment; null = the exercise's own (generator exercises only). */
    generator: GeneratorSettings | null;
    /** UTC. */
    assignedAt: string;
}

export interface IAssignExerciseRequest {
    exerciseId: number;
    learnerIds: number[];
    groupIds: number[];
    dueDate: string | null;
    allowDotArray: boolean;
    allowTimesTableMatrix: boolean;
    generator: GeneratorSettings | null;
}

export interface IRevokeAssignmentRequest {
    id: number;
}
