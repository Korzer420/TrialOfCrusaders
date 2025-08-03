using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders.Controller;

public abstract class GameModeController
{
    public abstract GameMode Mode { get; }

    public abstract string Explanation { get; }

    public virtual void SetupTreasurePool() => TreasureManager.TreasurePool = [..TreasureManager.Powers.Select(x => x.Name)];

    public virtual void OnStart() { }

    public virtual void OnEnd() { }

    /// <summary>
    /// Returns whether the <see cref="OnEnd"/> should be initiated.
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckForEnding() => true;

    public abstract List<RoomData> GenerateRoomList(bool atStart);
}
