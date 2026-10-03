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

/** Six digits, valid for 10 minutes; pairingUrl goes into the QR code (LP-106). */
export interface IPairingCode {
    code: string;
    /** ISO date-time. */
    expiresAt: string;
    pairingUrl: string;
}

/** A paired device in the parents' area. Times are ISO date-times (UTC). */
export interface IDevice {
    id: number;
    name: string;
    pairedAt: string;
    lastSeenAt?: string | null;
    expiresAt: string;
}

export interface IRevokeDeviceRequest {
    id: number;
}
