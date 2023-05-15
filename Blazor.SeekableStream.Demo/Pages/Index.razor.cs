using Microsoft.JSInterop;

namespace Blazor.SeekableStream.Demo.Pages;

sealed partial class Index : IAsyncDisposable
{
    const int RandomReadSize = 5 * 1024; // 5KB
    const string NA = "N/A";

    string? error;
    bool useSeekable = true;

    IJSSeekableStreamReference? seekable;
    IJSStreamReference? nonseekable;
    Stream? stream;
    bool? isStreamSeekable;

    long? length;
    string bytes = NA;

    async Task PickAFileAsync()
    {
        try
        {
            const string PickMethodName = "pickFileAsync";

            error = null;
            isStreamSeekable = null;
            await CleanUpAsync();

            var mod = await Js.InvokeAsync<IJSObjectReference>(
                "import",
                "/interop.js");

            if (useSeekable)
            {
                var file = await mod.InvokeAsync<IJSObjectReference>(PickMethodName);
                seekable = file.AsSeekableStreamReference(Js);
                stream = await seekable.OpenReadStreamAsync();
            }
            else
            {
                nonseekable = await mod.InvokeAsync<IJSStreamReference>(PickMethodName);
                stream = await nonseekable.OpenReadStreamAsync(long.MaxValue);
            }

            length = stream.Length;
            bytes = "N/A";
            isStreamSeekable = stream.CanSeek;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            await CleanUpAsync();
        }
    }

    async Task ReadRandomAsync()
    {
        ArgumentNullException.ThrowIfNull(stream);

        error = null;

        try
        {
            var random = new Random();
            var start = random.Next(0, (int)stream.Length);
            stream.Seek(start, SeekOrigin.Begin);

            var arr = new byte[RandomReadSize];
            var actual = await stream.ReadAsync(arr);

            bytes = $"Reading {RandomReadSize} bytes from {start}, {actual} bytes actually read:\r\n" +
                string.Join(' ', arr.Take(actual));

            if (bytes.Length > 100)
            {
                bytes = bytes[..100] + "…";
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
    }

    async Task CleanUpAsync()
    {
        if (seekable is not null)
        {
            await seekable.DisposeAsync();
            seekable = null;
        }

        if (nonseekable is not null)
        {
            await nonseekable.DisposeAsync();
            nonseekable = null;
        }

        if (stream is not null)
        {
            await stream.DisposeAsync();
            stream = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CleanUpAsync();
    }
}
