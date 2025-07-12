using TrialOfCrusaders.SaveData;

namespace TrialOfCrusaders;

/// <summary>
/// Interfac
/// </summary>
internal interface ISaveData
{
    /// <summary>
    /// Receives the save data to read from.
    /// <para/>Called whenever a controller is enabled (before enabling).
    /// </summary>
    public void ReceiveSaveData(LocalSaveData saveData);

    /// <summary>
    /// Modify the passed object with your updated values if desired.
    /// <para/>Called whenever a save is initiated or a controller is disabled.
    /// </summary>
    public void UpdateSaveData(LocalSaveData saveData);
}
