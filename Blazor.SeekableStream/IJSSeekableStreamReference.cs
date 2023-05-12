namespace Blazor.SeekableStream;

public interface IJSSeekableStreamReference : IAsyncDisposable
{
    Task<Stream> OpenReadStreamAsync();
}
