using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Blazor.SeekableStream;

internal class JSSeekableStreamReference : IJSSeekableStreamReference
{
    readonly IJSObjectReference jsRef;
    readonly IJSRuntime js;

    public JSSeekableStreamReference(IJSObjectReference jsRef, IJSRuntime js)
    {
        this.jsRef = jsRef;
        this.js = js;
    }

    public async ValueTask DisposeAsync()
    {
        await jsRef.DisposeAsync();
    }

    public async Task<Stream> OpenReadStreamAsync() =>
        await JsSeekableStream.CreateAsync(jsRef, js);

}

internal class JsSeekableStream : Stream
{
    static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name
        ?? throw new InvalidDataException("Cannot get Assembly FullName");

    bool disposed = false;
    readonly IJSObjectReference jsRef;

    long length;

    JsSeekableStream(IJSObjectReference jsRef)
    {
        this.jsRef = jsRef;
    }

    public static async Task<JsSeekableStream> CreateAsync(IJSObjectReference jsRef, IJSRuntime js)
    {
        var module = await js.InvokeAsync<IJSObjectReference>(
            "import",
            $"/_content/{AssemblyName}/SeekableStreamReferenceInterop.js");

        var jsStream = await module.InvokeAsync<IJSObjectReference>(
            "createStreamReference",
            jsRef);

        var result = new JsSeekableStream(jsStream)
        {
            length = await jsStream.InvokeAsync<long>("getLength"),
        };

        return result;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => length;
    public override long Position { get; set; }

    void ThrowIfDisposed()
    {
        if (disposed) { throw new ObjectDisposedException(null); }
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new InvalidOperationException("Call ReadAsync instead.");

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var bytes = await jsRef.InvokeAsync<byte[]>("readAsync", Position, buffer.Length);
        bytes.CopyTo(buffer);

        return bytes.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        ThrowIfDisposed();
        if (!CanSeek) { throw new InvalidOperationException(); }

        Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new NotSupportedException(),
        };

        if (Position < 0 || Position >= length)
        {
            throw new IndexOutOfRangeException($"Seek moved Position to {Position}, which is not within 0 and {length}");
        }

        return Position;
    }

    protected override async void Dispose(bool disposing)
    {
        if (disposed) { return; }
        disposed = true;

        base.Dispose(disposing);
        await jsRef.DisposeAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (disposed) { return; }
        disposed = true;

        await jsRef.DisposeAsync();
        await base.DisposeAsync();
    }

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Flush() => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

}
