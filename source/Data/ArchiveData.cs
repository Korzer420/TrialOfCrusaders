using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Data;

public class ArchiveData
{
    #region Properties

    public int TotalRuns { get; set; }

    public int HighestTrialScore { get; set; }

    public int HighestGrandTrialScore { get; set; }

    public float FastestTrial { get; set; }

    public float FastestGrandTrial { get; set; }

    public List<ArchiveEntryData> ArchiveEntries { get; set; } = [];

    public int GrubRecord { get; set; }

    public int EssenceRecord { get; set; }

    public bool CommonOnlyRun { get; set; }

    public bool FinishedSeededRun { get; set; }

    public bool PerfectFinalBoss { get; set; }

    public List<string> DebuffsSeen { get; set; } = new();

    #endregion

    public void AddPowerData(string powerName, bool picked)
    {
        if (!TreasureManager.Powers.Any(x => x.Name == powerName))
            return;
        ArchiveEntryData entry = ArchiveEntries.FirstOrDefault(x => x.PowerName == powerName);
        if (entry == null)
        {
            entry = new()
            {
                PowerName = powerName
            };
            ArchiveEntries.Add(entry);
        }
        entry.OfferedAmount++;
        if (picked)
            entry.PickedAmount++;
    }
}
