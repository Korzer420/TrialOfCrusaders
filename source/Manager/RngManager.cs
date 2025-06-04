using System;
using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders.Manager;

public static class RngManager
{
    private static int _seed = 120;
    private static Random _mainGenerator;
    private static (int, Random) _stageGenerator = new(-5000, new());

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

    public static int GetStageRandom(int minIncluded, int maxIncluded)
    {
        if (_stageGenerator.Item1 != StageController.CurrentRoomIndex)
            _stageGenerator = new(StageController.CurrentRoomIndex, new(_seed + StageController.CurrentRoomIndex));
        return _stageGenerator.Item2.Next(minIncluded, maxIncluded + 1);
    }

    public static float GetStageRandom(float minIncluded, float maxExcluded)
    {
        if (_stageGenerator.Item1 != StageController.CurrentRoomIndex)
            _stageGenerator = new(StageController.CurrentRoomIndex, new(_seed + StageController.CurrentRoomIndex));
        return (float)(_mainGenerator.NextDouble() * (maxExcluded - minIncluded) - minIncluded);
    }
}
