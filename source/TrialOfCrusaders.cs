using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;
using Caching = TrialOfCrusaders.Powers.Common.Caching;


namespace TrialOfCrusaders;

public class TrialOfCrusaders : Mod
{
    private Dummy _coroutineHolder;

    public static TrialOfCrusaders Instance { get; set; }

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
        TreasureController.Shiny = chest.transform.Find("Item").GetChild(0).gameObject;
        TreasureController.Shiny.name = "ToC Item";
        Component.Destroy(TreasureController.Shiny.GetComponent<ObjectBounce>());
        Component.Destroy(TreasureController.Shiny.GetComponent<PersistentBoolItem>());
        GameObject.DontDestroyOnLoad(TreasureController.Shiny);

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

        On.UIManager.StartNewGame += UIManager_StartNewGame;
        On.UIManager.ContinueGame += UIManager_ContinueGame;
        On.UIManager.ReturnToMainMenu += UIManager_ReturnToMainMenu;
        On.GameManager.GetStatusRecordInt += GameManager_GetStatusRecordInt;
        On.GameManager.StartNewGame += GameManager_StartNewGame;
        On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.OnEnter += PlayerDataBoolTest_OnEnter;
        if (_coroutineHolder != null)
            GameObject.Destroy(_coroutineHolder.gameObject);
        _coroutineHolder = new GameObject("TrialCoroutineHelper").AddComponent<Dummy>();
        GameObject.DontDestroyOnLoad(_coroutineHolder.gameObject);
    }

    private void PlayerDataBoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.PlayerDataBoolTest.orig_OnEnter orig, PlayerDataBoolTest self)
    {
        if (self.IsCorrectContext("Spell Control", "Knight", "Set HP Amount*"))
        { 
            TreasureController.SpawnShiny(TreasureType.EnduranceOrb, HeroController.instance.transform.position);
            ScoreController.DisplayScore();
        }
        orig(self);
    }

    private int GameManager_GetStatusRecordInt(On.GameManager.orig_GetStatusRecordInt orig, GameManager self, string key)
    {
        // If the selection mode menu doesn't appear, a few unity errors are thrown. Therefore we force it to appear.
        if (key == "RecPermadeathMode")
            return 1;
        return orig(self, key);
    }

    private void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
    {
        //Spawner.Load();
        //HubController.Initialize();
        //self.ContinueGame();
        orig(self, permadeathMode, bossRushMode);
        PDHelper.CorniferAtHome = true;
        PDHelper.GiantFlyDefeated = true;
        PDHelper.ZoteDead = true;
        PDHelper.GiantBuzzerDefeated = true;
        PDHelper.FountainVesselSummoned = true;
        PDHelper.HasKingsBrand = true;
        PDHelper.DuskKnightDefeated = true;
        // ToDo: Call OnHook (like IC to allow mods to modify).
    }

    private void UIManager_StartNewGame(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        orig(self, permaDeath, bossRush);
    }

    private void UIManager_ContinueGame(On.UIManager.orig_ContinueGame orig, UIManager self)
    {
        orig(self);
    }

    private IEnumerator UIManager_ReturnToMainMenu(On.UIManager.orig_ReturnToMainMenu orig, UIManager self)
    {
        yield return orig(self);
    }
}
