using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Controller;

public static class PhaseController
{
    public static Phase CurrentPhase { get; set; }

    internal static void Initialize()
    {
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;
        On.GameManager.StartNewGame += GameManager_StartNewGame;
    }

    private static void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
    {
        //Spawner.ContinueSpawn = false;
        // ToDo: Check game mode
        SpawnController.Initialize();
        self.ContinueGame();
        HubController.Initialize();
        HistoryController.Initialize();
        PDHelper.CorniferAtHome = true;
        PDHelper.ColosseumBronzeOpened = true;
        PDHelper.GiantFlyDefeated = true;
        PDHelper.ZoteDead = true;
        PDHelper.GiantBuzzerDefeated = true;
        PDHelper.FountainVesselSummoned = true;
        PDHelper.HasKingsBrand = true;
        PDHelper.DuskKnightDefeated = true;
        PDHelper.KilledInfectedKnight = true;
        PDHelper.KilledMageKnight = true;
        PDHelper.MegaMossChargerDefeated = true;
        PDHelper.InfectedKnightDreamDefeated = true;
        PDHelper.AbyssGateOpened = true;
        PDHelper.HegemolDefeated = true;
        CurrentPhase = Phase.Lobby;

        // ToDo: Call OnHook (like IC to allow mods to modify).
        //orig(self, permadeathMode, bossRushMode);
    }

    private static void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        //Spawner.ContinueSpawn = true;
        // ToDo: Check for savefile gamemode.
        SpawnController.Initialize();
        CurrentPhase = Phase.Lobby;
        orig(self);
        HubController.Initialize();
    }

    private static IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        // Save forfeited run.
        if (CurrentPhase == Phase.Run)
            HistoryController.AddEntry(RunResult.Forfeited);
        if (CurrentPhase != Phase.Inactive)
        {
            HistoryController.Unload();
            SpawnController.Unload();
            HubController.Unload();
            CombatController.Unload();
            StageController.Unload();
            ScoreController.Unload();
            TrialOfCrusaders.Holder.StopAllCoroutines();
        }
        CurrentPhase = Phase.Inactive;
        yield return orig(self);
    }
}
