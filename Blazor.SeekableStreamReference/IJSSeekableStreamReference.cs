namespace Blazor.SeekableStreamReference;

public interface IJSSeekableStreamReference : IAsyncDisposable
{
    Task<Stream> OpenReadStreamAsync();
}
