/** Mirrors the C# DTOs of SubjectsController (Web.Core). */
export interface ISubject {
    id: number;
    name: string;
    /** BCP 47, e.g. "de" – the language used for reading aloud. */
    languageCode: string;
    /** Design-token key, e.g. "subject.math" (see src/lib/subjects/subjectStyles.ts). */
    color: string;
    /** Icon key, e.g. "math". */
    icon: string;
}

export interface ISubjectInput {
    name: string;
    languageCode: string;
    color: string;
    icon: string;
}

export interface IUpdateSubjectRequest extends ISubjectInput {
    id: number;
}
