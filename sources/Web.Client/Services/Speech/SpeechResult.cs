namespace Web.Client.Services.Speech;

/// <summary>Outcome of one read-aloud request, with timings for the LP-007 measurements.</summary>
/// <param name="Error">Web Speech error code (e.g. <c>interrupted</c>, <c>not-allowed</c>) or <c>no-start</c>.</param>
/// <param name="StartDelayMs">Time from the request until the voice started; <c>null</c> if it never started.</param>
public sealed record SpeechResult(bool Ok, string? Error, double? StartDelayMs, double DurationMs, string? VoiceName);
