using Blazor.SeekableStream;

namespace Microsoft.JSInterop;

public static class SeekableStreamReferenceExtensions
{

    public static IJSSeekableStreamReference AsSeekableStreamReference(
        this IJSObjectReference reference,
        IJSRuntime jsRuntime) =>
        new JSSeekableStreamReference(reference, jsRuntime);

}
