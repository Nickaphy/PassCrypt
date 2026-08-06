using Microsoft.JSInterop;
public class ClipboardService : IAsyncDisposable
{
    // The binding between our Javascript and c#
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public ClipboardService(IJSRuntime js)
    {
        _moduleTask = new(() => js.InvokeAsync<IJSObjectReference>(
            "import", "./js/Clipboard.js").AsTask());
    }

    public async Task CopyText(string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("copyText", value);
    }

    public async Task ScheduleClear(string value, int delayMs)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("scheduleClear", value, delayMs);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}

