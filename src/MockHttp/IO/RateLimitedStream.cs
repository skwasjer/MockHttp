using System.Diagnostics;
using MockHttp.Threading;

namespace MockHttp.IO;

/// <summary>
/// A stream that rate limits read IO for a provided stream, which can be used to simulate network transfer rates with large data streams.
/// <para>
/// While reading from the stream, the read throughput is averaged over time and throttled to match the requested bit rate as close as possible.
/// </para>
/// </summary>
/// <remarks>
/// - Does not support writing.
/// - Do not use in real world applications, only tests, kty!
/// - Not 100% accurate (just like real world :p).
/// </remarks>
public class RateLimitedStream : Stream
{
    internal const int MinBitRate = 128;
    private const int SampleRate = 100; // How often to take samples (per sec).
    private const int MaxBufferSize = 2 << 15; // 128KB
    private static readonly TimeSpan ThrottleInterval = TimeSpan.FromMilliseconds(1000D / SampleRate);

    private readonly Stream _actualStream;
    private readonly int _byteRate;
    private readonly bool _leaveOpen;
    private readonly Stopwatch _stopwatch;

    private long _totalBytesRead;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitedStream" />.
    /// </summary>
    /// <param name="actualStream">The actual stream to wrap.</param>
    /// <param name="bitRate">The bit rate to simulate.</param>
    /// <param name="leaveOpen"><see langword="true" /> to leave the <paramref name="actualStream" /> open on close/disposal.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="actualStream" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the stream is not readable.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the bit rate is less than 128.</exception>
    public RateLimitedStream(Stream actualStream, int bitRate, bool leaveOpen = false)
    {
        _actualStream = actualStream ?? throw new ArgumentNullException(nameof(actualStream));
        if (!_actualStream.CanRead)
        {
            throw new ArgumentException("Cannot read from stream.", nameof(actualStream));
        }

        if (bitRate < MinBitRate)
        {
            throw new ArgumentOutOfRangeException(nameof(bitRate), $"Bit rate must be higher than or equal to {MinBitRate}.");
        }

        _leaveOpen = leaveOpen;
        _byteRate = bitRate / 8; // We are computing bytes transferred.
        _stopwatch = new Stopwatch();
    }

    /// <inheritdoc />
    public override bool CanRead => _actualStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => _actualStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => false;

    /// <inheritdoc />
    public override long Length => _actualStream.Length;

    /// <inheritdoc />
    public override long Position
    {
        get => _actualStream.Position;
        set => _actualStream.Position = value;
    }

    /// <inheritdoc />
    public override void Flush()
    {
        _actualStream.Flush();
    }

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count)
    {
        _stopwatch.Start();

        // Do not read more from stream than what is allowed per sec and/or capped.
        int chokeByteCount = Math.Min(Math.Min(count, _byteRate), MaxBufferSize);

        int bytesRead = _actualStream.Read(buffer, offset, chokeByteCount);
        if (bytesRead == 0)
        {
            return 0;
        }

        _totalBytesRead += bytesRead;
        double currentByteRate;
        do
        {
            currentByteRate = _totalBytesRead / _stopwatch.Elapsed.TotalSeconds;
            HighResDelay.Wait(ThrottleInterval);
        } while (currentByteRate > _byteRate);

        return bytesRead;
    }

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin)
    {
        return _actualStream.Seek(offset, origin);
    }

    /// <inheritdoc />
    public override void SetLength(long value)
    {
        throw new NotSupportedException("Modifying stream is not supported.");
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Modifying stream is not supported.");
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (!_leaveOpen)
            {
                _actualStream.Dispose();
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}
