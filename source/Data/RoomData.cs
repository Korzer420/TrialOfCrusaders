using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Data;

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

    public bool BigRoom { get; set; }

    #endregion

    #region Methods

    internal bool Available(bool easyMode, Progress progress, int currentRoom)
    {
        if (BossRoom)
        {
            return NeededProgress switch
            {
                Progress.None => true,
                Progress.Dash => currentRoom > (HubController.SelectedGameMode == GameMode.GrandCrusader ? 30 : 20) && progress.HasFlag(Progress.Dash | Progress.Fireball) || progress.HasFlag(Progress.Dash | Progress.Quake),
                Progress.Claw => currentRoom > (HubController.SelectedGameMode == GameMode.GrandCrusader ? 60 : 40) && progress.HasFlag(Progress.ShadeCloak | Progress.Wings | Progress.Fireball) || progress.HasFlag(Progress.ShadeCloak | Progress.Wings | Progress.Quake),
                // Special flag for endboss (Radiance, Pure Vessel, NKG)
                _ => false
            };
        }
        else
        {
            bool available = progress.HasFlag(NeededProgress) && (ConditionalProgress?.Count == 0 || ConditionalProgress.Any(x => progress.HasFlag(x)));
            if (!easyMode)
                return available;
            return available && progress.HasFlag(EasyNeededProgress) && (EasyConditionalProgress?.Count == 0 || EasyConditionalProgress.Any(x => progress.HasFlag(x)));
        }
    }

    #endregion
}
