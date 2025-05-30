using DebugMod;
using KorzUtils.Helper;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;

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
        if (PhaseController.CurrentPhase != Enums.Phase.Run)
        {
            Console.AddLine("Can't open gates (the game is not in the correct phase.)");
            return;
        }
        else if (StageController.CurrentRoom?.BossRoom == true)
        {
            Console.AddLine("Gate function not available in boss scenes. Spawn a shiny to initiate a transition.");
            return;
        }
        StageController.ClearExit();
        Console.AddLine("Open Gates");
    }

    [BindableMethod(name = "Spawn Power Orb", category = "TrialOfCrusaders")]
    public static void SpawnPowerOrb() => SpawnShiny(TreasureType.NormalOrb);

    [BindableMethod(name = "Spawn Rare Power Orb", category = "TrialOfCrusaders")]
    public static void SpawnRarePowerOrb() => SpawnShiny(TreasureType.RareOrb);

    [BindableMethod(name = "Spawn Combat Orb", category = "TrialOfCrusaders")]
    public static void SpawnCombatOrb() => SpawnShiny(TreasureType.CombatOrb);

    [BindableMethod(name = "Spawn Spirit Orb", category = "TrialOfCrusaders")]
    public static void SpawnSpiritOrb() => SpawnShiny(TreasureType.SpiritOrb);

    [BindableMethod(name = "Spawn Endurance Orb", category = "TrialOfCrusaders")]
    public static void SpawnEnduranceOrb() => SpawnShiny(TreasureType.EnduranceOrb);

    [BindableMethod(name = "Spawn Stat Orb", category = "TrialOfCrusaders")]
    public static void SpawnStatOrb() => SpawnShiny(TreasureType.PrismaticOrb);

    [BindableMethod(name = "Spawn Catch Up Orb", category = "TrialOfCrusaders")]
    public static void SpawnCatchUpOrb() => SpawnShiny(TreasureType.CatchUpStat);

    private static void SpawnShiny(TreasureType type)
    {
        if (PhaseController.CurrentPhase != Phase.Run)
        {
            Console.AddLine("Cannot spawn shiny outside an active trial.");
            return;
        }
        TreasureManager.SpawnShiny(type, HeroController.instance.transform.position, false);
        Console.AddLine("Spawn shiny at player position.");
    }
}
