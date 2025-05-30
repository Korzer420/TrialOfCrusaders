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
        ("GG_Atrium", "GG_Challenge_Door (1)/Door/Unlocked Set/Inspect"),
        ("Room_Fungus_Shaman", "Scream Control/Scream Item"),
        ("Ruins_Bathhouse", "Ghost NPC/Idle Pt")
    ];

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Instance = this;
        base.Initialize(preloadedObjects);

        // Create a global coroutine handler that all functions in the mod can use (to keep it independend of other behaviors)
        if (_coroutineHolder != null)
            GameObject.Destroy(_coroutineHolder.gameObject);
        _coroutineHolder = new GameObject("Coroutine Helper").AddComponent<Dummy>();

        // Handle the preloaded objects.
        OrganizePrefabs(preloadedObjects);

        // Hook other mods for interops
        if (ModHooks.GetMod("DebugMod") is Mod)
            HookDebug();

        PhaseController.Initialize();
        On.GameManager.GetStatusRecordInt += EnsureSteelSoul;
    }

    private int EnsureSteelSoul(On.GameManager.orig_GetStatusRecordInt orig, GameManager self, string key)
    {
        // If the selection mode menu doesn't appear, a few unity errors are thrown. Therefore we force it to appear.
        if (key == "RecPermadeathMode")
            return 1;
        return orig(self, key);
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

    private void SetupPowerPrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
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
        GameObject.DontDestroyOnLoad(GroundSlam.Shockwave);

        GreaterMind.Orb = preloadedObjects["Ruins1_23"]["Mage"].GetComponent<PersonalObjectPool>().startupPool[0].prefab;
        GameObject.DontDestroyOnLoad(GreaterMind.Orb);

        Caching.SoulCache = preloadedObjects["Ruins1_23"]["Ruins Vial Empty (2)/Active/soul_cache (1)"];
        GameObject.DontDestroyOnLoad(Caching.SoulCache);

        VoidZone.Ring = preloadedObjects["GG_Hollow_Knight"]["Battle Scene/HK Prime/Focus Blast/focus_ring"];
        GameObject.DontDestroyOnLoad(VoidZone.Ring);
    }

    private void SetupDebuffs(Dictionary<string, Dictionary<string, GameObject>> objects)
    {
        ConcussionEffect.PreparePrefab(objects["Deepnest_43"]["Mantis Heavy Flyer"].GetComponent<PersonalObjectPool>().startupPool[0].prefab);
        WeakenedEffect.PreparePrefab(objects["Room_Fungus_Shaman"]["Scream Control/Scream Item"]);
        ShatteredMindEffect.PreparePrefab(objects["Ruins_Bathhouse"]["Ghost NPC/Idle Pt"]);
        // We love carl <3
        GameObject carl = objects["Crossroads_ShamanTemple"]["_Enemies/Zombie Runner"];
        BleedEffect.PreparePrefab(typeof(InfectedEnemyEffects).GetField("spatterOrangePrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(carl.GetComponent<InfectedEnemyEffects>()) as GameObject);

        GameObject corpse = typeof(EnemyDeathEffects).GetField("corpsePrefab", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(carl.GetComponent<EnemyDeathEffects>()) as GameObject;
        BurnEffect.PreparePrefab(corpse.transform.Find("Corpse Flame").gameObject);
    }

    private void OrganizePrefabs(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        GameObject container = GameObject.Find("Trial of Crusaders Objects");
        if (container != null)
            GameObject.Destroy(container);
        container = new("Trial of Crusaders Objects");
        GameObject.DontDestroyOnLoad(container);

        // Setup prefabs
        SetupPowerPrefabs(preloadedObjects);
        SetupDebuffs(preloadedObjects);
        HubController.Tink = preloadedObjects["Deepnest_43"]["Mantis Heavy Flyer"].GetComponent<PersonalObjectPool>().startupPool[0].prefab.GetComponent<TinkEffect>().blockEffect;
        Gate.Prefab = preloadedObjects["Deepnest_East_10"]["Dream Gate"];
        TreasureManager.SetupShiny(preloadedObjects["Tutorial_01"]["_Props/Chest"]);
        ScoreController.SetupScoreboard(preloadedObjects["GG_Atrium"]["GG_Challenge_Door (1)/Door/Unlocked Set/Inspect"]);
        SpecialTransition.SetupPrefab(preloadedObjects["GG_Workshop"]["GG_Statue_Vengefly/Inspect"]);
        ScoreController.SetupResultInspect(preloadedObjects["GG_Workshop"]["GG_Statue_Vengefly/Inspect"]);

        GameObject[] preloads = 
        [
            // Power prefabs
            ..LifebloodOmen.Ghosts,
            GroundSlam.Shockwave,
            GreaterMind.Orb,
            Caching.SoulCache,
            VoidZone.Ring,
            // Debuffs
            ConcussionEffect.Prefab,
            WeakenedEffect.Prefab,
            ShatteredMindEffect.Prefab,
            BleedEffect.Prefab,
            BurnEffect.Prefab,
            // Other
            HubController.Tink,
            Gate.Prefab,
            TreasureManager.Shiny,
            ScoreController.ResultSequencePrefab,
            ScoreController.ScoreboardPrefab,
            SpecialTransition.TransitionPrefab,
            _coroutineHolder.gameObject
        ];
        foreach (GameObject gameObject in preloads)
        {
            gameObject.transform.SetParent(container.transform);
            GameObject.DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }
        _coroutineHolder.gameObject.SetActive(true);
    }
}
