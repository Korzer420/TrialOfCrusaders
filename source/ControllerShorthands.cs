using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Manager;

namespace TrialOfCrusaders;

public static class ControllerShorthands
{
    #region Controller Shorthand

    /// <summary>
    /// Shorthand to <see cref="CombatController"/>.
    /// </summary>
    public static CombatController CombatRef => PhaseManager.GetController<CombatController>();

    /// <summary>
    /// Shorthand to <see cref="HistoryController"/>.
    /// </summary>
    public static HistoryController HistoryRef => PhaseManager.GetController<HistoryController>();

    /// <summary>
    /// Shorthand to <see cref="ConsumableController"/>.
    /// </summary>
    public static ConsumableController ConsumableRef => PhaseManager.GetController<ConsumableController>();

    /// <summary>
    /// Shorthand to <see cref="HubController"/>.
    /// </summary>
    public static HubController HubRef => PhaseManager.GetController<HubController>();

    /// <summary>
    /// Shorthand to <see cref="StageController"/>.
    /// </summary>
    public static StageController StageRef => PhaseManager.GetController<StageController>();

    /// <summary>
    /// Shorthand to <see cref="SecretController"/>.
    /// </summary>
    public static SecretController SecretRef => PhaseManager.GetController<SecretController>();

    /// <summary>
    /// Shorthand to <see cref="SecretController"/>.
    /// </summary>
    public static ScoreController ScoreRef => PhaseManager.GetController<ScoreController>();

    /// <summary>
    /// Shorthand to <see cref="InventoryController"/>.
    /// </summary>
    public static InventoryController InventoryRef => PhaseManager.GetController<InventoryController>();

    #endregion
}
