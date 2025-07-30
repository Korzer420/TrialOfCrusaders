using KorzUtils.Helper;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.SaveData;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.Manager;

/// <summary>
/// Handles the transition between phases so the correct controllers are active.
/// </summary>
public static class PhaseManager
{
    private static readonly List<BaseController> _controller = 
    [
        new CombatController(),
        new ConsumableController(),
        new HistoryController(),
        new HubController(),
        new InventoryController(),
        new ScoreController(),
        new SecretController(),
        new SpawnController(),
        new StageController(),
        new PowerController()
    ];

    public delegate void PhaseChange(Phase currentPhase, Phase newPhase);

    public static Phase CurrentPhase { get; set; }

    public static GameModeController CurrentGameMode { get; set; }

    internal static event PhaseChange PhaseChanged;

    /// <summary>
    /// Adds the controller to the controller list.
    /// <para/>Returns true, if the controller was successfully added.
    /// <para/>Only one controller per type can be added.
    /// </summary>
    public static bool AddController(BaseController controller)
    {
        if (_controller.Any(x => x.GetType() == controller.GetType()))
            return false;
        _controller.Add(controller);
        return true;
    }

    public static bool RemoveController(BaseController controller) => _controller.RemoveAll(x => x.GetType() == controller.GetType()) > 0;

    public static T GetController<T>() where T : BaseController
    {
        // No Linq for faster access.
        foreach (BaseController controller in _controller)
            if (controller.GetType() == typeof(T))
                return controller as T;
        return null;
    }

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
            PDHelper.ColosseumGoldOpened = true;
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
            PlayerData.instance.corn_deepnestLeft = true;
            HistoryRef.History = [];
        }
        else
            HistoryRef.History = null;
    }

    private static IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        LogManager.Log("Return to menu");
        if (CurrentPhase != Phase.Inactive)
            TransitionTo(Phase.WaitForSave);
        yield return orig(self);
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
        PhaseChanged?.Invoke(CurrentPhase, targetPhase);
        if (targetPhase == Phase.Inactive || targetPhase == Phase.Lobby || targetPhase == Phase.Result)
        {
            TrialOfCrusaders.Holder.StopAllCoroutines();
            if (targetPhase == Phase.Inactive || targetPhase == Phase.Lobby)
                CurrentGameMode = null;
        }
        try
        {
            foreach (BaseController controller in _controller)
                if (controller.GetActivePhases().Contains(targetPhase))
                    controller.TryEnable();
                else
                    controller.TryDisable();
        }
        catch (Exception ex)
        {
            LogManager.Log($"Failed to transition from {CurrentPhase} to {targetPhase}", ex);
        }
        CurrentPhase = targetPhase;
    }

    internal static void CollectSaveData(LocalSaveData saveData)
    {
        foreach (BaseController controller in _controller)
            if (controller is ISaveData save)
                save.UpdateSaveData(saveData);
    }
}
