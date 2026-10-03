/** RFC 9457 error body returned by Web.Core (ASP.NET Core ProblemDetails / ValidationProblemDetails). */
export type ProblemDetails = {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    /** Validation errors per field (ValidationProblemDetails). */
    errors?: Record<string, string[]>;
};
