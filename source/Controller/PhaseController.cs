using KorzUtils.Helper;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Handles the transition between phases so the right controllers are active.
/// </summary>
public static class PhaseController
{
    public static Phase CurrentPhase { get; set; }

    internal static void Initialize()
    {
        On.GameManager.ContinueGame += GameManager_ContinueGame;
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;
        On.GameManager.StartNewGame += GameManager_StartNewGame;
        IL.GameManager.StartNewGame += SkipStartRoutine;
    }

    #region Game stats events

    private static void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        TransitionTo(Phase.Listening);
        orig(self);
    }

    private static void GameManager_ContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
    {
        orig(self);
        if (CurrentPhase == Phase.Initialize)
            TransitionTo(Phase.Lobby);
        else
            TransitionTo(Phase.Inactive);
    }

    private static void SkipStartRoutine(ILContext il)
    {
        ILCursor cursor = new(il);
        ILLabel label;
        cursor.GotoNext(x => x.MatchRet());
        cursor.GotoNext(x => x.MatchRet());
        label = cursor.MarkLabel();
        cursor.Goto(0);
        cursor.EmitDelegate(() => CurrentPhase != Phase.Inactive);
        cursor.Emit(OpCodes.Brtrue, label);
    }

    private static void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
    {
        //Spawner.ContinueSpawn = false;
        // ToDo: Check game mode
        // The IL hook above should prevent the original code from running.
        // We still keep this call to allow other mods to trigger.
        orig(self, permadeathMode, bossRushMode);
        if (CurrentPhase == Phase.Initialize)
        {
            self.ContinueGame();
            PDHelper.CorniferAtHome = true;
            PDHelper.ColosseumBronzeCompleted = true;
            PDHelper.ColosseumSilverOpened = true;
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
            HistoryController.History = [];
        }
        else 
            HistoryController.History = null;
    }

    private static IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        TransitionTo(Phase.Inactive);
        yield return orig(self);
        HistoryController.History = null;
    }

    #endregion

    internal static void TransitionTo(Phase targetPhase)
    {
        if (CurrentPhase == targetPhase)
        {
            if (CurrentPhase != Phase.Inactive)
                LogManager.Log("Phase controller is already in phase " + targetPhase, KorzUtils.Enums.LogType.Warning);
            return;
        }
        LogManager.Log("Transition to phase: " + targetPhase);
        try
        {
            switch (targetPhase)
            {
                case Phase.Run:
                    if (CurrentPhase == Phase.Lobby)
                    {
                        StageController.Initialize();
                        ScoreController.Initialize();
                        CombatController.Initialize();
                        HistoryController.Unload();
                        CoroutineHelper.WaitUntil(() =>
                        {
                            PDHelper.HasDreamNail = false;
                            GameObject pedestal = new("Pedestal");
                            pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Pedestal");
                            pedestal.transform.position = new(104.68f, 15.4f, 0);
                            pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
                            pedestal.layer = 8; // Terrain layer
                            pedestal.SetActive(true);

                            pedestal = new("Pedestal2");
                            pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Pedestal");
                            pedestal.transform.position = new(109f, 15.4f, 0);
                            pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
                            pedestal.layer = 8; // Terrain layer
                            pedestal.SetActive(true);
                            // Spawn two orbs at the start.
                            TreasureManager.SpawnShiny(TreasureType.PrismaticOrb, new(104.68f, 20.4f), false);
                            TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(109f, 20.4f), false);
                            PhaseController.CurrentPhase = Phase.Run;
                            HubController.Unload();
                        }, () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GG_Spa", true);
                    }
                    else
                        LogManager.Log("Invalid transition. " + CurrentPhase + " -> " + targetPhase);
                    break;
                case Phase.Result:
                    if (CurrentPhase == Phase.Run)
                    {
                        HistoryController.AddEntry(RunResult.Completed);
                        StageController.Unload();
                        CombatController.Unload();
                    }
                    else
                        LogManager.Log("Invalid transition. " + CurrentPhase + " -> " + targetPhase);
                    break;
                case Phase.Inactive:
                    // Save forfeited run.
                    if (CurrentPhase == Phase.Run)
                        HistoryController.AddEntry(RunResult.Forfeited);
                    HistoryController.Unload();
                    SpawnController.Unload();
                    HubController.Unload();
                    CombatController.Unload();
                    StageController.Unload();
                    ScoreController.Unload();
                    TrialOfCrusaders.Holder.StopAllCoroutines();
                    break;
                case Phase.Lobby:
                    HubController.Initialize();
                    HistoryController.Initialize();
                    if (CurrentPhase != Phase.Inactive && CurrentPhase != Phase.Initialize)
                    {
                        if (CurrentPhase == Phase.Run)
                        {
                            // Reset shade, save history entry and reset to hub control.
                            PDHelper.ShadeMapZone = string.Empty;
                            PDHelper.ShadeScene = string.Empty;
                            PDHelper.ShadePositionX = 0;
                            PDHelper.ShadePositionY = 0;
                            PDHelper.SoulLimited = false;
                            ScoreController.Score.Score = PDHelper.GeoPool;
                            HistoryController.AddEntry(RunResult.Failed);
                            CombatController.Unload();
                            StageController.Unload();
                        }
                        PDHelper.GeoPool = 0;
                        ScoreController.Unload();
                    }
                    else
                        SpawnController.Initialize();
                    break;
                default:
                    break;
            }
        }
        catch (System.Exception ex)
        {
            LogManager.Log($"Failed to transition from {CurrentPhase} to {targetPhase}", ex);
        }
        CurrentPhase = targetPhase;
    }
}
