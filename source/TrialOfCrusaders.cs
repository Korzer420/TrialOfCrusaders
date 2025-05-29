using HutongGames.PlayMaker.Actions;
using KorzUtils.Helper;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.ModInterop;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.SaveData;
using TrialOfCrusaders.UnityComponents;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;
using Caching = TrialOfCrusaders.Powers.Common.Caching;


namespace TrialOfCrusaders;

public class TrialOfCrusaders : Mod, ILocalSettings<LocalSaveData>
{
    private Dummy _coroutineHolder;

    public static TrialOfCrusaders Instance { get; set; }

    public bool RunActive { get; set; }

    internal static Dummy Holder => Instance._coroutineHolder;

    public override List<(string, string)> GetPreloadNames() =>
    [
        ("Tutorial_01", "_Props/Chest"),
        ("Deepnest_43", "Mantis Heavy Flyer"),
        ("Crossroads_ShamanTemple", "_Enemies/Zombie Runner"),
        ("Ruins1_28", "Flamebearer Spawn"), // Small Ghost
        ("RestingGrounds_06", "Flamebearer Spawn"), // Medium Ghost
        ("Hive_03", "Flamebearer Spawn"), // Large Ghost
        ("Ruins1_24_boss", "Mage Lord"),
        ("Ruins1_23", "Mage"),
        ("Ruins1_23", "Ruins Vial Empty (2)/Active/soul_cache (1)"),
        ("GG_Workshop", "GG_Statue_Vengefly/Inspect"),
        ("Deepnest_East_10", "Dream Gate"),
        ("GG_Hollow_Knight", "Battle Scene/HK Prime/Focus Blast/focus_ring"),
        ("GG_Atrium", "GG_Challenge_Door (1)/Door/Unlocked Set/Inspect")
    ];

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Instance = this;
        base.Initialize(preloadedObjects);
        GameObject chest = preloadedObjects["Tutorial_01"]["_Props/Chest"];
        TreasureManager.Shiny = chest.transform.Find("Item").GetChild(0).gameObject;
        TreasureManager.Shiny.name = "ToC Item";
        Component.Destroy(TreasureManager.Shiny.GetComponent<ObjectBounce>());
        Component.Destroy(TreasureManager.Shiny.GetComponent<PersistentBoolItem>());
        GameObject.DontDestroyOnLoad(TreasureManager.Shiny);

        ConcussionEffect.ConcussionObject = preloadedObjects["Deepnest_43"]["Mantis Heavy Flyer"].GetComponent<PersonalObjectPool>().startupPool[0].prefab;
        GameObject.DontDestroyOnLoad(ConcussionEffect.ConcussionObject);

        // We love carl <3
        GameObject carl = preloadedObjects["Crossroads_ShamanTemple"]["_Enemies/Zombie Runner"];

        GameObject corpse = typeof(EnemyDeathEffects).GetField("corpsePrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(carl.GetComponent<EnemyDeathEffects>()) as GameObject;
        BurnEffect.Burn = corpse.transform.Find("Corpse Flame").gameObject;
        GameObject.DontDestroyOnLoad(BurnEffect.Burn);

        BleedEffect.Bleed = typeof(InfectedEnemyEffects).GetField("spatterOrangePrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(carl.GetComponent<InfectedEnemyEffects>()) as GameObject;
        GameObject.DontDestroyOnLoad(BleedEffect.Bleed);

        GameObject ghost = preloadedObjects["Ruins1_28"]["Flamebearer Spawn"]
            .LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
        GameObject.DontDestroyOnLoad(ghost);
        LifebloodOmen.Ghosts.Add(ghost);

        ghost = preloadedObjects["RestingGrounds_06"]["Flamebearer Spawn"]
            .LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
        GameObject.DontDestroyOnLoad(ghost);
        LifebloodOmen.Ghosts.Add(ghost);

        ghost = preloadedObjects["Hive_03"]["Flamebearer Spawn"]
            .LocateMyFSM("Spawn Control").FsmVariables.FindFsmGameObject("Grimmkin Obj").Value;
        GameObject.DontDestroyOnLoad(ghost);
        LifebloodOmen.Ghosts.Add(ghost);

        GroundSlam.Shockwave = preloadedObjects["Ruins1_24_boss"]["Mage Lord"].LocateMyFSM("Mage Lord")
            .GetState("Quake Waves")
            .GetFirstAction<SpawnObjectFromGlobalPool>()
            .gameObject.Value;

        GreaterMind.Orb = preloadedObjects["Ruins1_23"]["Mage"].GetComponent<PersonalObjectPool>().startupPool[0].prefab;
        GameObject.DontDestroyOnLoad(GreaterMind.Orb);

        StageController.TransitionObject = preloadedObjects["GG_Workshop"]["GG_Statue_Vengefly/Inspect"];
        GameObject.DontDestroyOnLoad(StageController.TransitionObject);

        Caching.SoulCache = preloadedObjects["Ruins1_23"]["Ruins Vial Empty (2)/Active/soul_cache (1)"];
        GameObject.DontDestroyOnLoad(Caching.SoulCache);
        Gate.Prefab = preloadedObjects["Deepnest_East_10"]["Dream Gate"];
        GameObject.DontDestroyOnLoad(Gate.Prefab);

        VoidZone.Ring = preloadedObjects["GG_Hollow_Knight"]["Battle Scene/HK Prime/Focus Blast/focus_ring"];
        GameObject.DontDestroyOnLoad(VoidZone.Ring);

        ScoreController.Prefab = preloadedObjects["GG_Atrium"]["GG_Challenge_Door (1)/Door/Unlocked Set/Inspect"]
            .LocateMyFSM("Challenge UI").GetState("Open UI").GetFirstAction<ShowBossDoorChallengeUI>().prefab.Value;
        GameObject.DontDestroyOnLoad(ScoreController.Prefab);

        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;
        On.GameManager.GetStatusRecordInt += EnsureSteelSoul;
        On.GameManager.StartNewGame += GameManager_StartNewGame;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += SpawnShiny;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += StageController.SceneManager_activeSceneChanged;
        if (_coroutineHolder != null)
            GameObject.Destroy(_coroutineHolder.gameObject);
        _coroutineHolder = new GameObject("TrialCoroutineHelper").AddComponent<Dummy>();
        GameObject.DontDestroyOnLoad(_coroutineHolder.gameObject);

        if (ModHooks.GetMod("DebugMod") is Mod)
            HookDebug();
    }

#if DEBUG

    private void SpawnShiny(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Set HP Amount*"))
        {
            TreasureManager.SpawnShiny(TreasureType.NormalOrb, HeroController.instance.transform.position);
        }
        orig(self);
    } 

#endif

    private int EnsureSteelSoul(On.GameManager.orig_GetStatusRecordInt orig, GameManager self, string key)
    {
        // If the selection mode menu doesn't appear, a few unity errors are thrown. Therefore we force it to appear.
        if (key == "RecPermadeathMode")
            return 1;
        return orig(self, key);
    }

    private void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
    {
        //Spawner.ContinueSpawn = false;
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
        // ToDo: Call OnHook (like IC to allow mods to modify).
        //orig(self, permadeathMode, bossRushMode);
    }

    private void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        //Spawner.ContinueSpawn = true;
        SpawnController.Initialize();
        orig(self);
        HubController.Initialize();
    }

    private IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        // Save forfeited run.
        if (RunActive)
            HistoryController.AddEntry(RunResult.Forfeited);
        RunActive = false;
        HistoryController.Unload();
        SpawnController.Unload();
        HubController.Unload(); 
        CombatController.Unload();
        StageController.Unload();
        ScoreController.Unload();
        Holder.StopAllCoroutines();
        yield return orig(self);
    }

    private void HookDebug()
    {
        DebugModInterop.Initialize();
    }

    void ILocalSettings<LocalSaveData>.OnLoadLocal(LocalSaveData saveData)
    {
        HistoryController.SetupList(saveData?.OldRunData ?? []);
        // ToDo: Support save inside a run.
        //StageController.CurrentRoomIndex = CurrentSaveData.CurrentRoomNumber - 2;
        //List<Power> powers = [];

        //CombatController.ObtainedPowers = [.. CurrentSaveData.ObtainedPowers.Select(x => TreasureController.Powers.First(y => x == y.Name))];
        //CombatController.CombatLevel = saveData.CombatLevel;
        //CombatController.SpiritLevel = saveData.SpiritLevel;
        //CombatController.EnduranceLevel = saveData.EnduranceLevel;
        //StageController.CurrentRoomData = [..saveData.RoomList.Select(x => new RoomData()
        //{
        //    Name = x.Split('[')[0],
        //    SelectedTransition = x.Split('[')[1].Split(']')[0]
        //})];
        //ScoreController.Score = saveData.Score.Copy();
        //RngProvider.Seed = saveData.RandomSeed;
        //PDHelper.HasDash = saveData.CurrentProgress.HasFlag(Progress.Dash);
        //PDHelper.CanDash = PDHelper.HasDash;
        //PDHelper.HasWalljump = saveData.CurrentProgress.HasFlag(Progress.Claw);
        //PDHelper.HasDoubleJump = saveData.CurrentProgress.HasFlag(Progress.Wings);
        //PDHelper.HasShadowDash = saveData.CurrentProgress.HasFlag(Progress.ShadeCloak);
        //PDHelper.HasSuperDash = saveData.CurrentProgress.HasFlag(Progress.CrystalHeart);
        //PDHelper.HasAcidArmour = saveData.CurrentProgress.HasFlag(Progress.Tear);
        //PDHelper.HasLantern = saveData.CurrentProgress.HasFlag(Progress.Lantern);
    }

    LocalSaveData ILocalSettings<LocalSaveData>.OnSaveLocal()
    {
        // ToDo: Support save inside a run.
        // We have three save stages:
        // In the lobby (CurrentRoomIndex is -2 or -1): Take only non-run data.
        // In room 40 or 80 (in the normal run): Save ALL data.
        // In any other room: Only save run fixed run data + power flags (all obtained powers and stats are kept from the old version).
        //if (StageController.CurrentRoomIndex < 0)
        //    CurrentSaveData = CurrentSaveData.GetLobbyData();
        //else if (StageController.CurrentRoomNumber % 40 != 0)
        //    CurrentSaveData = CurrentSaveData.GetFixedData();
        //else
        //    CurrentSaveData = CurrentSaveData.GetUpdatedData();
        return new() { OldRunData = HistoryController.History };
    }
}
