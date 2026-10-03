/** Mirrors the C# DTOs of DevicesController (Web.Core). */
export interface IDeviceStatus {
    isPaired: boolean;
    familyName?: string;
}

export interface IPairDeviceRequest {
    code: string;
    deviceName: string;
}

export interface IPairDeviceResponse {
    familyName: string;
}

export interface IDeviceProfile {
    learnerId: number;
    firstName: string;
    avatarId: string;
}

export interface IDeviceSignInRequest {
    learnerId: number;
}
