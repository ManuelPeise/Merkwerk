namespace Logic.Devices.Pairing;

public enum PairStatus
{
    Success,

    /// <summary>Unknown, used, replaced or expired – deliberately one answer.</summary>
    InvalidCode,
}
