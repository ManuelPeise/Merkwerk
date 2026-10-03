namespace Logic.Devices.Sessions;

public enum LearnerSignInStatus
{
    Success,

    /// <summary>No device token, or the device was unpaired or has expired.</summary>
    DeviceNotPaired,

    /// <summary>No such child in the device's family.</summary>
    LearnerNotFound,
}
