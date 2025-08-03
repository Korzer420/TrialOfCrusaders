namespace TrialOfCrusaders.SaveData;

public class GlobalSaveData
{
    #region Properties

    public bool TrackForfeitedRuns { get; set; } = true;

    public bool TrackFailedRuns { get; set; } = true;

    public int HistoryAmount { get; set; } = 20;

    public bool UseCustomSprites { get; set; }

    #endregion
}
