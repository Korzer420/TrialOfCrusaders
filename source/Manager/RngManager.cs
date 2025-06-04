using System;

namespace TrialOfCrusaders.Manager;

public static class RngManager
{
    private static int _seed = 120;
    private static Random _mainGenerator = new(120);

    public static int Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            _mainGenerator = new(value);
        }
    }

    internal static bool Seeded { get; set; }

    public static int GetRandom(int minIncluded, int maxIncluded) => _mainGenerator.Next(minIncluded, maxIncluded + 1);

    public static float GetRandom(float minIncluded, float maxExcluded) => (float)(_mainGenerator.NextDouble() * (maxExcluded - minIncluded) - minIncluded);
}
