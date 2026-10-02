using Microsoft.JSInterop;

namespace Web.Client.Services.Speech;

/// <summary>Read-aloud through the browser's Web Speech API (js/speech.js). Runs on the device only.</summary>
public sealed class SpeechService(IJSRuntime js) : IAsyncDisposable
{
    private const int VoiceTimeoutMs = 3000;
    private const int StartTimeoutMs = 5000;

    private Task<IJSObjectReference>? _module;

    public async Task<bool> IsSupportedAsync(CancellationToken cancellationToken = default) =>
        await (await ModuleAsync(cancellationToken)).InvokeAsync<bool>("isSupported", cancellationToken);

    public async Task<IReadOnlyList<SpeechVoice>> GetVoicesAsync(CancellationToken cancellationToken = default) =>
        await (await ModuleAsync(cancellationToken)).InvokeAsync<SpeechVoice[]>("getVoices", cancellationToken, VoiceTimeoutMs);

    /// <summary>Speaks <paramref name="text"/> and completes when speaking has ended. A new call interrupts the previous one.</summary>
    public async Task<SpeechResult> SpeakAsync(
        string text,
        string lang,
        string? voiceUri = null,
        double rate = 1.0,
        CancellationToken cancellationToken = default) =>
        await (await ModuleAsync(cancellationToken)).InvokeAsync<SpeechResult>(
            "speak", cancellationToken, text, lang, voiceUri, rate, StartTimeoutMs);

    public async Task CancelAsync(CancellationToken cancellationToken = default) =>
        await (await ModuleAsync(cancellationToken)).InvokeVoidAsync("cancel", cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await (await _module).DisposeAsync();
        }
    }

    private Task<IJSObjectReference> ModuleAsync(CancellationToken cancellationToken) =>
        _module ??= js.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/speech.js").AsTask();
}
