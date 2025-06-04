using System.Collections.Generic;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Data;

/// <summary>
/// Used to show old runs.
/// </summary>
public class HistoryData
{
    public string ModVersion { get; set; }

    public RunResult Result { get; set; }

    public GameMode GameMode { get; set; }

    public bool Seeded { get; set; }

    public int Seed { get; set; }

    public int RunId { get; set; }

    public ScoreData Score { get; set; }

    public int FinalCombatLevel { get; set; }

    public int FinalSpiritLevel { get; set; }

    public int FinalEnduranceLevel { get; set; }

    public int FinalRoomNumber { get; set; }

    public int CommonPowerAmount { get; set; }

    public int UncommonPowerAmount { get; set; }

    public int RarePowerAmount { get; set; }

    public List<string> Powers { get; set; } = [];

    /// <summary>
    /// Check if the assigned run id matches the values.
    /// If not, the run data might've been tempered with.
    /// </summary>
    internal bool CheckRun() => true; /*GetRunId() == RunId;*/

    internal int GetRunId() => 1;
    // ToDo: Implement run id verifier.
    //internal int GetRunId() => ($"{ModVersion};{Result};{GameMode};{Seeded};{Seed};{Score.Score};{Score.EssenceBonus};{Score.KillStreakBonus};" +
    //           $"{Score.HighestHitlessRoomStreak};{Score.PerfectBossesBonus};{Score.HitlessFinalBoss};{Score.TotalHitlessRooms};" +
    //           $"{FinalCombatLevel}:{FinalSpiritLevel};{FinalEnduranceLevel}:{FinalRoomNumber};" +
    //           $"{CommonPowerAmount};{UncommonPowerAmount};{RarePowerAmount};{string.Join(",", Powers)}").GetHashCode();
}
