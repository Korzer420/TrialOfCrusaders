using DebugMod;
using TrialOfCrusaders.Controller;

namespace TrialOfCrusaders.ModInterop;

public static class DebugModInterop
{
    #region Control

    internal static void Initialize() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugModInterop));

    #endregion

    /// <summary>
    /// Adds a bomb bag. Can exceed the normal limit.
    /// </summary>
    [BindableMethod(name = "Open Gates", category = "TrialOfCrusaders")]
    public static void OpenGates()
    {
        StageController.ClearExit();
        DebugMod.Console.AddLine("Open Gates");
    }

    [BindableMethod(name = "Print Enemies", category = "TrialOfCrusaders")]
    public static void PrintEnemies()
    {
        foreach (HealthManager enemy in CombatController.Enemies)
            DebugMod.Console.AddLine("Enemy name: "+enemy.name);

    }

}
