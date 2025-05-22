using System;
using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders;

public static class RngProvider
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

    public static int GetProgressRandom(int minIncluded, int maxIncluded)
    {
        Random random = new(_seed + StageController.CurrentRoomIndex + CombatController.SpiritLevel * 10 + CombatController.CombatLevel * 201 + CombatController.EnduranceLevel * 2006);
        return random.Next(minIncluded, maxIncluded + 1);
    }

    public static float GetProgressRandom(float minIncluded, float maxExcluded)
    {
        Random random = new(_seed + StageController.CurrentRoomIndex + CombatController.SpiritLevel * 10 + CombatController.CombatLevel * 201 + CombatController.EnduranceLevel * 2006);
        return (float)(random.NextDouble() * (maxExcluded - minIncluded) - minIncluded);
    }
}
