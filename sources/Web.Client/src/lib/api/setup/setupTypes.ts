/** Mirrors the C# DTOs of SetupController (Web.Core). */
export interface ISetupStatus {
    isSetupRequired: boolean;
}

export interface ISetupInitializeRequest {
    familyName: string;
    displayName: string;
    email: string;
    password: string;
    privacyAccepted: boolean;
}
