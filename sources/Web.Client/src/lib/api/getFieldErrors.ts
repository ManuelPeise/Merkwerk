import type { ProblemDetails } from 'src/lib/api/types/problemDetails';

const toCamelCase = (value: string): string => value.charAt(0).toLowerCase() + value.slice(1);

/**
 * ProblemDetails.errors → { field: first message }. Field names are camelCase and without a
 * prefix like "request.", so they match the form model.
 */
export const getFieldErrors = (problem: ProblemDetails | undefined): Record<string, string> => {
    const fieldErrors: Record<string, string> = {};

    for (const [key, messages] of Object.entries(problem?.errors ?? {})) {
        const field = toCamelCase(key.split('.').pop() ?? key);

        if (messages.length > 0 && !(field in fieldErrors)) {
            fieldErrors[field] = messages[0];
        }
    }

    return fieldErrors;
};
