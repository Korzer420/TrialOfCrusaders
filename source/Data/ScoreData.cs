namespace TrialOfCrusaders.Data;

public class ScoreData
{
    private int _currentHitlessRoomStreak;
    private int _currentKillStreak;

    #region Properties

    public int CurrentHitlessRoomStreak
    {
        get => _currentHitlessRoomStreak;
        set
        {
            _currentHitlessRoomStreak = value;
            if (value > HighestHitlessRoomStreak)
                HighestHitlessRoomStreak = value;
        }
    }

    public int CurrentKillStreak
    {
        get => _currentKillStreak;
        set
        {
            _currentKillStreak = value;
            if (value > _currentKillStreak)
                HighestKillStreak = value;
        }
    }

    public int HitlessBosses { get; set; }

    public int TotalHitlessRooms { get; set; }

    public int HighestHitlessRoomStreak { get; internal set; }

    public int HighestKillStreak { get; internal set; }

    public float PassedTime { get; set; }

    #endregion

    public ScoreData Copy() => new()
    {
        CurrentHitlessRoomStreak = CurrentHitlessRoomStreak,
        CurrentKillStreak = CurrentKillStreak,
        HitlessBosses = HitlessBosses,
        TotalHitlessRooms = TotalHitlessRooms,
        HighestHitlessRoomStreak = HighestHitlessRoomStreak,
        HighestKillStreak = HighestKillStreak,
        PassedTime = PassedTime
    };
}
