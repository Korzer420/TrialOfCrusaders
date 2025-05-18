using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders;

public class RoomData
{
    #region Properties

    public string Name { get; set; }

    public List<string> AllowedEntrances { get; set; } = [];

    public bool BossRoom { get; set; }

    public Progress NeededProgress { get; set; }

    public List<Progress> ConditionalProgress { get; set; } = [];

    public Progress EasyNeededProgress { get; set; }

    public List<Progress> EasyConditionalProgress { get; set; } = [];

    [JsonIgnore]
    public string SelectedTransition { get; set; } = "door_dreamEnter";

    [JsonIgnore]
    public bool IsQuietRoom => SelectedTransition == "Warp";

    #endregion

    #region Methods

    internal bool Available(bool easyMode, Progress progress, int currentRoom)
    {
        if (BossRoom)
        {
            if (currentRoom < 20)
                return false;
            return NeededProgress switch
            {
                Progress.None => true,
                Progress.Dash => currentRoom > 30 && progress.HasFlag(Progress.Dash | Progress.Fireball) || progress.HasFlag(Progress.Dash | Progress.Quake),
                Progress.Claw => currentRoom > 60 && progress.HasFlag(Progress.ShadeCloak | Progress.Wings | Progress.Fireball) || progress.HasFlag(Progress.ShadeCloak | Progress.Wings | Progress.Quake),
                // Special flag for endboss (Radiance, Pure Vessel, NKG)
                _ => false
            };
        }
        else
        {
            // Each 20th room is guaranteed a boss.
            if (currentRoom % 20 == 0)
                return false;
            bool available = progress.HasFlag(NeededProgress) && (ConditionalProgress?.Count == 0 || ConditionalProgress.Any(x => progress.HasFlag(x)));
            if (!easyMode)
                return available;
            return available && progress.HasFlag(EasyNeededProgress) && (EasyConditionalProgress?.Count == 0 || EasyConditionalProgress.Any(x => progress.HasFlag(x)));
        }
    }

    #endregion
}
