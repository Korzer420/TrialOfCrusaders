using System.Collections.Generic;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.SaveData;

namespace TrialOfCrusaders.Manager;

/// <summary>
/// Manages all save data related things.
/// </summary>
internal static class SaveManager
{
    internal static LocalSaveData CurrentSaveData { get; set; }
}