/** Mirrors the C# DTOs of GroupsController (Web.Core). */
export interface IGroup {
    id: number;
    name: string;
    /** Children in the group (ids of ILearner), sorted. */
    learnerIds: number[];
}

export interface IGroupInput {
    name: string;
    /** Exactly the children the group should hold (may be empty). */
    learnerIds: number[];
}

export interface IUpdateGroupRequest extends IGroupInput {
    id: number;
}

export interface IDeleteGroupRequest {
    id: number;
}
