namespace Web.Client.Services.Speech;

/// <summary>A voice offered by the device's speech engine.</summary>
public sealed record SpeechVoice(string Name, string Lang, string VoiceUri, bool LocalService, bool IsDefault);
