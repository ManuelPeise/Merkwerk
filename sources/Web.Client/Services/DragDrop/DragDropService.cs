using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Web.Client.Services.DragDrop;

/// <summary>
/// Wraps SortableJS (js/dragdrop.js). The element must have <c>data-zone</c>, its children <c>data-id</c> and a <c>@key</c>.
/// The DOM is never changed by JS: drops are reported via <c>onDrop</c> and the component re-renders.
/// </summary>
public sealed class DragDropService(IJSRuntime js) : IAsyncDisposable
{
    private Task<IJSObjectReference>? _module;

    public async Task<IAsyncDisposable> AttachAsync(
        ElementReference element,
        DragDropOptions options,
        Func<DropEvent, Task> onDrop,
        CancellationToken cancellationToken = default)
    {
        _module ??= js.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./js/dragdrop.js").AsTask();
        var module = await _module;

        var callback = DotNetObjectReference.Create(new DropCallback(onDrop));
        var handle = await module.InvokeAsync<IJSObjectReference>(
            "attach",
            cancellationToken,
            element,
            callback,
            new { group = options.Group, sort = options.AllowSort, touchDelayMs = options.TouchDelayMs });

        return new Attachment(handle, callback);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await (await _module).DisposeAsync();
        }
    }

    internal sealed class DropCallback(Func<DropEvent, Task> onDrop)
    {
        [JSInvokable]
        public Task OnDrop(string itemId, string fromZone, string toZone, int newIndex) =>
            onDrop(new DropEvent(itemId, fromZone, toZone, newIndex));
    }

    private sealed class Attachment(IJSObjectReference handle, DotNetObjectReference<DropCallback> callback) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await handle.InvokeVoidAsync("dispose");
            await handle.DisposeAsync();
            callback.Dispose();
        }
    }
}
