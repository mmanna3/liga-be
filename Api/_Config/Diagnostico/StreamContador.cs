namespace Api._Config.Diagnostico;

/// <summary>
/// Stream de paso ("passthrough") que sólo cuenta los bytes escritos.
/// Se usa para saber cuántos bytes se enviaron realmente por la red
/// (después de compresión) sin alterar la respuesta.
/// </summary>
public sealed class StreamContador : Stream
{
    private readonly Stream _inner;

    public StreamContador(Stream inner) => _inner = inner;

    public long BytesEscritos { get; private set; }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;
    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _inner.FlushAsync(cancellationToken);
    public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        BytesEscritos += count;
        _inner.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        BytesEscritos += count;
        return _inner.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        BytesEscritos += buffer.Length;
        return _inner.WriteAsync(buffer, cancellationToken);
    }
}
