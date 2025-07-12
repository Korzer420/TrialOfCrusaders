using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders;

/// <summary>
/// Base class for all classes/manager which are permanently active during certain phases.
/// </summary>
public abstract class BaseController
{
    private bool _enabled;

    #region Methods

    internal void TryEnable()
    {
        if (_enabled)
            return;
        try
        {
            if (this is ISaveData saveData)
                saveData.ReceiveSaveData(SaveManager.CurrentSaveData);
            Enable();
            _enabled = true;
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Couldn't enable controller. ", ex);
        }
    }

    internal void TryDisable()
    {
        if (!_enabled)
            return;
        try
        {
            if (this is ISaveData saveData)
                saveData.UpdateSaveData(SaveManager.CurrentSaveData);
            Disable();
            _enabled = false;
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Couldn't disable controller. ", ex);
        }
    }

    /// <summary>
    /// Enables the controller.
    /// </summary>
    protected abstract void Enable();

    /// <summary>
    /// Disables the controller.
    /// <para/>Use this method to reset the entire controller.
    /// </summary>
    protected abstract void Disable();

    /// <summary>
    /// Get all phases where this controller is active.
    /// If you need additional info on what phase change occured, subscribe to <see cref="PhaseManager.PhaseChanged"/>.
    /// <para/>PhaseChanged is called BEFORE the mod checks if the controller should be active.
    /// </summary>
    public abstract Phase[] GetActivePhases(); 

    #endregion
}
