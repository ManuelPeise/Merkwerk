/** Mirrors the C# DTOs of LearnersController (Web.Core). */
export interface ILearner {
    id: number;
    displayName: string;
    /** 1-4. */
    grade: number;
    avatarId: string;
}

export interface ILearnerInput {
    displayName: string;
    grade: number;
    avatarId: string;
}

export interface IUpdateLearnerRequest extends ILearnerInput {
    id: number;
}

export interface IDeleteLearnerRequest {
    id: number;
}
