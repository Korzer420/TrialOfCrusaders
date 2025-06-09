using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrialOfCrusaders.Data;

public class ScoreData
{
    private int _currentKillStreak;

    internal const string ScoreField = "Score:";
    internal const string TimeField = "Time bonus:";
    internal const string KillStreakField = "Kill streak bonus:";
    internal const string EssenceField = "Essence bonus:";
    internal const string GrubField = "Grub bonus:";
    internal const string TraverseField = "Traverse bonus:";
    internal const string PerfectBossesField = "Perfect boss bonus:";
    internal const string PerfectFinalBossField = "Perfect final boss:";
    internal const string FinalScoreField = "Final score:";

    #region Properties

    public float PassedTime { get; set; }

    public int CurrentKillStreak
    {
        get => _currentKillStreak;
        set
        {
            _currentKillStreak = value;
            if (value > KillStreakBonus)
                KillStreakBonus = value;
        }
    }

    public int KillStreakBonus { get; set; }

    public int Score { get; set; }

    public int EssenceBonus { get; set; }

    public int TraverseBonus { get; set; }

    public int GrubBonus { get; set; }

    public int PerfectBossesBonus { get; set; }

    public bool HitlessFinalBoss { get; set; }

    // Currently not evaluated.
    public int TakenHits { get; set; }

    #endregion

    public ScoreData Copy() => new()
    {
        CurrentKillStreak = CurrentKillStreak,
        PerfectBossesBonus = PerfectBossesBonus,
        KillStreakBonus = KillStreakBonus,
        PassedTime = PassedTime,
        TakenHits = TakenHits,
        Score = Score,
        EssenceBonus = EssenceBonus,
        HitlessFinalBoss = HitlessFinalBoss,
        GrubBonus = GrubBonus,
        TraverseBonus = TraverseBonus
    };

    internal Dictionary<string, int> TransformToDictionary()
    {
        var values = TransformToList();
        return values.ToDictionary(x => x.Item1, x => x.Item2);
    }

    internal List<(string, int)> TransformToList()
    {
        List<(string, int)> scoreData =
        [
            new(ScoreField, Score),
            new(TimeField, Math.Max(0, Mathf.CeilToInt(7200 - PassedTime))),
            new(KillStreakField, KillStreakBonus * 5),
            new(EssenceField, EssenceBonus * 10),
            new(GrubField, GrubBonus * 50),
            new(TraverseField, TraverseBonus * 20),
            new(PerfectBossesField, PerfectBossesBonus * 200),
            new(PerfectFinalBossField, HitlessFinalBoss ? 2000 : 0),
        ];
        scoreData.Add(new(FinalScoreField, scoreData.Sum(x => x.Item2)));
        return scoreData;
    }
}
