using System.Globalization;
using MockHttp.IO;

// ReSharper disable once CheckNamespace
namespace MockHttp;

/// <summary>
/// Defines different types of bit rates to simulate a slow network.
/// </summary>
public sealed class BitRate
{
    private readonly Func<int> _factory;
    private readonly string _name;

    private BitRate(Func<int> factory, string name)
    {
        _factory = factory;
        _name = name;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{GetType().Name}.{_name}";
    }

    /// <summary>
    /// 2G (mobile network) bit rate. (~64kbps).
    /// </summary>
    public static BitRate TwoG()
    {
        return Create(64_000, nameof(TwoG));
    }

    /// <summary>
    /// 3G (mobile network) bit rate. (~2Mbps)
    /// </summary>
    public static BitRate ThreeG()
    {
        return Create(2_000_000, nameof(ThreeG));
    }

    /// <summary>
    /// 4G (mobile network) bit rate. (~64Mbps)
    /// </summary>
    public static BitRate FourG()
    {
        return Create(64_000_000, nameof(FourG));
    }

    /// <summary>
    /// 5G (mobile network) bit rate. (~512Mbps)
    /// </summary>
    public static BitRate FiveG()
    {
        return Create(512_000_000, nameof(FiveG));
    }

    /// <summary>
    /// 10 Mbps.
    /// </summary>
    public static BitRate TenMegabit()
    {
        return Create(10_000_000, nameof(TenMegabit));
    }

    /// <summary>
    /// 100 Mbps.
    /// </summary>
    public static BitRate OneHundredMegabit()
    {
        return Create(100_000_000, nameof(OneHundredMegabit));
    }

    /// <summary>
    /// 1 Gbps.
    /// </summary>
    public static BitRate OneGigabit()
    {
        return Create(1_000_000_000, nameof(OneGigabit));
    }

    /// <summary>
    /// Converts a bit rate to an integer representing the bit rate in bits per second.
    /// </summary>
    /// <param name="bitRate"></param>
    /// <returns></returns>
    public static explicit operator int (BitRate bitRate)
    {
        return ToInt32(bitRate);
    }

    /// <summary>
    /// Converts a bit rate to an integer representing the bit rate in bits per second.
    /// </summary>
    /// <param name="bitRate"></param>
    /// <returns></returns>
    public static explicit operator BitRate (int bitRate)
    {
        return FromInt32(bitRate);
    }

    /// <summary>
    /// Converts a bit rate to an integer representing the bit rate in bits per second.
    /// </summary>
    /// <param name="bitRate">The bit rate.</param>
    /// <returns>The underlying bit rate value.</returns>
    public static int ToInt32(BitRate bitRate)
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        return bitRate?._factory() ?? -1;
    }

    /// <summary>
    /// Convert an integer bit rate (in bits per second) to a <see cref="BitRate" />.
    /// </summary>
    /// <param name="bitRate">The bit rate.</param>
    /// <returns>The bit rate.</returns>
    public static BitRate FromInt32(int bitRate)
    {
        return Create(bitRate, FormatBps(bitRate));
    }

    private static BitRate Create(int bitRate, string name)
    {
        if (bitRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bitRate));
        }

        return new BitRate(() => bitRate, name);
    }

    private static string FormatBps(long value)
    {
        return BpsToString().ToString(CultureInfo.InvariantCulture);

        FormattableString BpsToString()
        {
            return value switch
            {
                < 1_000 => $"Around({value}bps)",
                < 1_000_000 => $"Around({(double)value / 1_000:#.##}kbps)",
                < 1_000_000_000 => $"Around({(double)value / 1_000_000:#.##}Mbps)",
                _ => $"Around({(double)value / 1_000_000_000:#.##}Gbps)"
            };
        }
    }
}
