namespace Logic.Content.Generators;

/// <summary>
/// Small deterministic random generator (Mulberry32) – same seed, same numbers, on every platform and .NET version
/// (<see cref="Random"/> makes no such promise). 32-bit only, so a later TypeScript port gives identical tasks.
/// </summary>
internal sealed class SeededRandom
{
    private uint _state;

    public SeededRandom(int seed)
    {
        _state = unchecked((uint)seed);
    }

    /// <summary>A number from <paramref name="minInclusive"/> to <paramref name="maxInclusive"/>.</summary>
    public int Next(int minInclusive, int maxInclusive)
    {
        if (maxInclusive < minInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxInclusive));
        }

        var range = (uint)(maxInclusive - minInclusive) + 1;
        return minInclusive + (int)(NextUInt() % range);
    }

    /// <summary>True in about half of the calls.</summary>
    public bool NextBool() => (NextUInt() & 1) == 1;

    private uint NextUInt()
    {
        unchecked
        {
            _state += 0x6D2B79F5;
            var t = _state;
            t = (t ^ (t >> 15)) * (t | 1);
            t ^= t + ((t ^ (t >> 7)) * (t | 61));
            return t ^ (t >> 14);
        }
    }
}
