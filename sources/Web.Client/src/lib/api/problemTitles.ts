/**
 * Fixed ProblemDetails titles the client tells apart when the status code alone is ambiguous
 * (mirrors Web.Core/Services/ApiControllers/ProblemTitles.cs).
 */
export const problemTitles = {
    emailNotConfirmed: 'E-mail not confirmed',
    noMembership: 'No membership',
} as const;
