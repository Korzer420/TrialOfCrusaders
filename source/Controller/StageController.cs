using GlobalEnums;
using HutongGames.PlayMaker;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.UnityComponents.StageElements;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

internal static class StageController
{
    private static bool _enabled;
    private static List<TransitionPoint> _transitionPoints = [];
    private static List<SpecialTransition> _specialTransitions = [];
    private static int _treasureRoomCooldown = 0;
    private static (string, string) _intendedDestination = new();
    private static TextMeshPro _roomCounter;

    #region Properties

    public static bool QuietRoom { get; set; } = true;

    public static bool UpcomingTreasureRoom { get; set; }

    public static int CurrentRoomIndex { get; set; } = 0;

    public static int CurrentRoomNumber => CurrentRoomIndex + 1;

    public static List<RoomData> CurrentRoomData { get; set; } = [];

    public static bool FinishedEnemies { get; set; }

    public static RoomData CurrentRoom => CurrentRoomIndex == -1 ? null : CurrentRoomData[CurrentRoomIndex];

    #endregion

    public static event Action<bool, bool> RoomEnded;

    #region Setup

    internal static void Initialize()
    {
        if (_enabled)
            return;
        LogManager.Log("Enable Stage Controller");
        QuietRoom = true;
        On.PlayMakerFSM.OnEnable += FsmEdits;
        On.RestBench.Start += RemoveBenches;

        // These hooks ensure that nothing is saved within a scene so it can repeated over an dover again.
        IL.SceneData.SaveMyState_GeoRockData += SkipSceneDataSave;
        IL.SceneData.SaveMyState_PersistentBoolData += SkipSceneDataSave;
        IL.SceneData.SaveMyState_PersistentIntData += SkipSceneDataSave;
        // Change flags to prevent softlocks (like already lowering platforms)
        On.SceneData.FindMyState_PersistentBoolData += ForceSceneFlags;

        // Transition handling.
        On.GameManager.BeginSceneTransition += InitiateTransition;
        On.TransitionPoint.Start += ModifyTransitionPoint;
        On.HeroController.FinishedEnteringScene += FinishedEnteringScene;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneAdjustments;

        CombatController.EnemiesCleared += OnEnemiesCleared;
        HistoryController.CreateEntry += PassHistoryData;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        var textElement = TextManager.CreateUIObject("RoomCounter");
        Component.Destroy(textElement.Item1);
        _roomCounter = textElement.Item2;
        _roomCounter.text = "Current room: 0";
        _roomCounter.transform.position = new(-14.5f, -9);
        _roomCounter.fontSize = 3;
        GameObject.DontDestroyOnLoad(_roomCounter.transform.parent);

        _enabled = true;
    }

    internal static void Unload()
    {
        if (!_enabled)
            return;
        LogManager.Log("Disable Stage Controller");
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        On.RestBench.Start -= RemoveBenches;

        // These hooks ensure that nothing is saved within a scene so it can repeated over an dover again.
        IL.SceneData.SaveMyState_GeoRockData -= SkipSceneDataSave;
        IL.SceneData.SaveMyState_PersistentBoolData -= SkipSceneDataSave;
        IL.SceneData.SaveMyState_PersistentIntData -= SkipSceneDataSave;
        // Change flags to prevent softlocks (like already lowering platforms)
        On.SceneData.FindMyState_PersistentBoolData -= ForceSceneFlags;

        // Transition handling.
        On.GameManager.BeginSceneTransition -= InitiateTransition;
        On.HeroController.FinishedEnteringScene -= FinishedEnteringScene;
        On.TransitionPoint.Start -= ModifyTransitionPoint;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneAdjustments;

        CombatController.EnemiesCleared -= OnEnemiesCleared;
        HistoryController.CreateEntry -= PassHistoryData;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;

        // Reset data.
        CurrentRoomIndex = -1;
        QuietRoom = true;
        _transitionPoints.Clear();
        _specialTransitions.Clear();
        CurrentRoomData.Clear();
        FinishedEnemies = false;
        GameObject.Destroy(_roomCounter.gameObject);
        _enabled = false;
    }

    #endregion

    #region Methods

    private static void SpawnTeleporter(Vector3 source, Vector3 target)
    {
        GameObject firstLift = new("Trial Lift");
        Lift first = firstLift.AddComponent<Lift>();
        firstLift.transform.position = source + new Vector3(0f, 0f, 0.01f);

        GameObject secondLift = new("Trial Lift 2");
        secondLift.AddComponent<Lift>().Partner = first;
        secondLift.transform.position = target + new Vector3(0f, 0f, 0.01f);
        first.Partner = secondLift.GetComponent<Lift>();
    }

    internal static void ClearExit()
    {
        if (!CurrentRoomData[CurrentRoomIndex].BossRoom)
        {
            foreach (TransitionPoint point in _transitionPoints)
                point.gameObject.GetComponent<Gate>()?.Revert(_specialTransitions.Count == 0);
            foreach (SpecialTransition item in _specialTransitions)
                item.GetComponent<BoxCollider2D>().isTrigger = true;
            PlayClearSound();
        }
    }

    internal static void EnableExit()
    {
        if (_specialTransitions.Count > 0)
        {
            _specialTransitions.First().GetComponent<BoxCollider2D>().isTrigger = true;
            _specialTransitions.First().WaitForItem = false;
        }
    }

    public static void PlayClearSound(bool room = true)
    {
        GameObject audioObject = new("Trial Room Clear");
        audioObject.SetActive(false);
        audioObject.transform.position = HeroController.instance.transform.position;
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.minDistance = 0;
        source.maxDistance = 5000;
        source.playOnAwake = false;
        // Needed so it does respect the audio settings.
        source.outputAudioMixerGroup = HeroController.instance.transform.Find("Attacks/Slash").GetComponent<AudioSource>().outputAudioMixerGroup;
        audioObject.SetActive(true);
        // Play as one shot so we can increase the volume
        string clipName = room ? "Room_Clear.wav" : "Run_Finished.wav";
        source.PlayOneShot(SoundHelper.CreateSound(ResourceHelper.LoadResource<TrialOfCrusaders>($"SoundEffects.{clipName}"), clipName), 2f);
        TrialOfCrusaders.Holder.StartCoroutine(CoroutineHelper.WaitUntil(() => UnityEngine.Object.Destroy(audioObject), () => source == null || !source.isPlaying));
    }

    internal static List<RoomData> LoadRoomData() => ResourceHelper.LoadJsonResource<TrialOfCrusaders, List<RoomData>>("Data.RoomData.json");

    internal static IEnumerator WaitForTransition()
    {
        HeroController.instance.RelinquishControl();
        float passedTime = 0f;
        while (passedTime < 2)
        {
            passedTime += Time.deltaTime;
            yield return null;
            if (GameManager.instance.IsGamePaused())
                yield return new WaitUntil(() => !GameManager.instance.IsGamePaused());
        }
        GameObject transition = new("Trial Transition Starter");
        SpecialTransition specialTransition = transition.AddComponent<SpecialTransition>();
        specialTransition.LoadIntoDream = false;
        transition.transform.position = new(-5000, -5000f);
        specialTransition.StartGodhomeTransition();
    }

    #endregion

    #region Transition Handling

    private static void InitiateTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        _transitionPoints.Clear();
        _specialTransitions.Clear();
        if (self.IsGameplayScene())
        {
            // Sometimes a scene is loaded twice. This should prevent skipping too far.
            if (_intendedDestination.Item1 != null)
            {
                info.SceneName = _intendedDestination.Item1;
                info.EntryGateName = _intendedDestination.Item2;
            }
            else
            {
                CombatController.ActiveEnemies.Clear();
                if (CurrentRoomNumber > 0)
                {
                    GatePosition entryPosition;
                    string entryGateName = CurrentRoom.SelectedTransition;
                    if (entryGateName.StartsWith("left"))
                        entryPosition = GatePosition.left;
                    else if (entryGateName.StartsWith("right"))
                        entryPosition = GatePosition.right;
                    else if (entryGateName.StartsWith("bot"))
                        entryPosition = GatePosition.bottom;
                    else if (entryGateName.StartsWith("top"))
                        entryPosition = GatePosition.top;
                    else if (entryGateName.StartsWith("door"))
                        entryPosition = GatePosition.door;
                    else
                        entryPosition = GatePosition.unknown;
                    RoomEnded?.Invoke(QuietRoom, !QuietRoom && info.HeroLeaveDirection != null
                        && info.HeroLeaveDirection != entryPosition && entryPosition != GatePosition.unknown);
                }

                FinishedEnemies = false;
                // Check for ending.
                if (CurrentRoomNumber == CurrentRoomData.Count)
                {
                    LogManager.Log("Trigger ending");
                    CurrentRoomIndex++;
                    QuietRoom = true;
                    info.EntryGateName = "left1";
                    info.SceneName = "Room_Colosseum_Bronze";
                    PhaseController.TransitionTo(Phase.Result);
                    info.Visualization = GameManager.SceneLoadVisualizations.Colosseum;
                    info.PreventCameraFadeOut = QuietRoom;
                    GameManager.instance.cameraCtrl.gameObject.LocateMyFSM("CameraFade").FsmVariables.FindFsmBool("No Fade").Value = QuietRoom;
                }
                else
                {

                    if (UpcomingTreasureRoom)
                    {
                        info.SceneName = "GG_Engine";
                        QuietRoom = true;
                        UpcomingTreasureRoom = false;
                    }
                    else
                    {
                        //// Test for specific room index.
                        //if (CurrentRoomIndex == 1)
                        //    CurrentRoomIndex = 46;

                        CurrentRoomIndex++;
                        QuietRoom = CurrentRoomData[CurrentRoomIndex].IsQuietRoom;
                        if (QuietRoom)
                            info.SceneName = "GG_Engine";
                        else
                            info.SceneName = CurrentRoomData[CurrentRoomIndex].Name;
                    }

                    if (QuietRoom || CurrentRoomData[CurrentRoomIndex].BossRoom)
                        info.EntryGateName = "door_dreamEnter";
                    else
                        info.EntryGateName = CurrentRoomData[CurrentRoomIndex].SelectedTransition;

                    info.Visualization = GameManager.SceneLoadVisualizations.Dream;
                    info.PreventCameraFadeOut = QuietRoom || CurrentRoom.BossRoom;
                    GameManager.instance.cameraCtrl.gameObject.LocateMyFSM("CameraFade").FsmVariables.FindFsmBool("No Fade").Value = QuietRoom || CurrentRoom.BossRoom;

                    // Treasure rooms can only appear under these conditions:
                    // Not later than 115.
                    // Not earlier than 10.
                    // Not after/before a quiet room.
                    if (_treasureRoomCooldown == 0 && !UpcomingTreasureRoom && CurrentRoomNumber >= 10 && CurrentRoomNumber <= CurrentRoomData.Count - 5
                        && !CurrentRoomData[CurrentRoomIndex - 1].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom)
                    {
                        float chance = 1f;
                        if (CombatController.HasPower<Damocles>(out _))
                            chance += 6f;
                        if (CombatController.HasPower<TreasureHunter>(out _))
                            chance += 2f;
                        UpcomingTreasureRoom = chance >= RngManager.GetRandom(0f, 100f);
                        if (UpcomingTreasureRoom)
                            _treasureRoomCooldown = 6;
                    }
                    else
                        _treasureRoomCooldown = _treasureRoomCooldown.Lower(1);
                    _intendedDestination = new(info.SceneName, info.EntryGateName);
                }
            }
            orig(self, info);
            // Prevent auto walk if left through dive.
            if (HeroController.instance != null)
                HeroController.instance.exitedQuake = false;
        }
    }

    private static void ModifyTransitionPoint(On.TransitionPoint.orig_Start orig, TransitionPoint self)
    {
        orig(self);
        _transitionPoints.Add(self);
        //if (Spawner.ContinueSpawn)
        //    QuietRoom = true;
        if (!QuietRoom)
        {
            Gate gate = self.gameObject.AddComponent<Gate>();
            CoroutineHelper.WaitForHero(() =>
            {
                if (self.gameObject.name.Contains("top"))
                    TrialOfCrusaders.Holder.StartCoroutine(gate.WaitForHero());
                else
                    gate.PlaceCollider();
            }, true);
        }
        if (QuietRoom || UpcomingTreasureRoom || (CurrentRoomIndex - 1 < CurrentRoomData.Count - 2 && (CurrentRoomData[CurrentRoomIndex + 1].BossRoom || CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom)))
        {
            if (self.isADoor)
                return;
            GameObject transitionObject = new("Trial Transition");
            SpecialTransition transition = transitionObject.AddComponent<SpecialTransition>();
            transition.LoadIntoDream = CurrentRoomData[CurrentRoomIndex + 1].BossRoom || CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom;
            transition.VanillaTransition = self;
            _specialTransitions.Add(transition);
        }
        if (QuietRoom && CurrentRoomIndex != -1 && self.name == "right1")
        {
            GameObject pedestal = new("Pedestal");
            pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Pedestal");
            pedestal.transform.position = new(94.23f, 14.8f, -0.1f);
            pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
            pedestal.layer = 8; // Terrain layer
            pedestal.SetActive(true);
            // If we are in a quiet room even though the room flag isn't set, we are in a treasure room instead.
            if (!CurrentRoomData[CurrentRoomIndex].IsQuietRoom)
                TreasureManager.SpawnShiny(RngManager.GetRandom(0, 100) < 10 ? TreasureType.RareOrb : TreasureType.NormalOrb, new(94.23f, 16.4f), false);
            else
            {
                // Special case for spells at 41 and 81. If they have been obtained already, they get replaced by a rare treasure guaranteed.
                if (CurrentRoomNumber == 41 || CurrentRoomNumber == 81)
                {
                    TreasureType intendedSpell = (TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name);
                    if (intendedSpell == TreasureType.Fireball && PDHelper.FireballLevel != 0 || intendedSpell == TreasureType.Quake && PDHelper.QuakeLevel != 0)
                    {
                        TreasureManager.SpawnShiny(TreasureType.RareOrb, new(94.23f, 16.4f), false);
                        return;
                    }
                    else
                        _specialTransitions.Last().WaitForItem = true;
                }
                else
                    _specialTransitions.Last().WaitForItem = true;
                TreasureManager.SpawnShiny((TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name), new(94.23f, 16.4f), false);
            }
        }
    }

    private static void FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        // Already triggered, we skip this.
        if (_intendedDestination.Item1 == null)
            return;
        //if (Spawner.ContinueSpawn)
        //    return;
        _intendedDestination.Item1 = null;
        if (GameManager.instance.sceneName == "Fungus3_40")
        {
            GameObject marmuGate = GameObject.Find("Dream Gate");
            if (marmuGate != null)
                GameObject.Destroy(marmuGate);
            marmuGate = GameObject.Find("Dream Gate (1)");
            if (marmuGate != null)
                GameObject.Destroy(marmuGate);
        }
        _roomCounter.text = $"Current room: {CurrentRoomNumber}";
        if (!QuietRoom)
            PlayMakerFSM.BroadcastEvent("DREAM GATE CLOSE");
        else if (QuietRoom && !CurrentRoom.IsQuietRoom)
            _roomCounter.text = "Treasure room";
        if (!CombatController.HasPower<DreamNail>(out _)
            && (GameManager.instance.sceneName == "Mines_05" || GameManager.instance.sceneName == "Mines_11" || GameManager.instance.sceneName == "Mines_37"))
            GameHelper.DisplayMessage("You can use your dream nail... temporarly.");
    }

    #endregion

    #region Other Eventhandler

    private static void RemoveBenches(On.RestBench.orig_Start orig, RestBench self)
    {
        orig(self);
        // To ensure other mods are running correctly, we just move the bench out of bounce.
        if (GameManager.instance?.RespawningHero != true)
            self.transform.position = new(-4000f, -4000f);
    }

    private static void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (self.FsmName == "Shiny Control" && self.gameObject.name.StartsWith("Shiny Item"))
            {
                self.AddState("Grant Geo", () => HeroController.instance.AddGeo(50), FsmTransitionData.FromTargetState("Flash").WithEventName("FINISHED"));
                self.GetState("Charm?").AdjustTransitions("Grant Geo");
            }
            else if (self.gameObject.name == "Ghost Warrior NPC" && (self.FsmName == "Appear" || self.FsmName == "Conversation Control"))
                self.GetState("Init").AdjustTransitions("Inert");
            else if (self.FsmName == "Vessel Fragment Control")
            {
                self.AddState("Grant Geo", () => { HeroController.instance.AddGeo(100); self.gameObject.SetActive(false); });
                FsmState state = self.GetState("Get");
                state.Actions =
                [
                    state.Actions[0], // Audio Stop
                state.Actions[1],
                //state.Actions[2], FSM CANCEL
                //state.Actions[3], Set invincibility
                state.Actions[4],
                state.Actions[5],
                state.Actions[6],
                //state.Actions[7], Relinquish Control
                //state.Actions[8], Stop Animation
                //state.Actions[9], Play hp gain animation
                //state.Actions[10], Set gravity scale
                state.Actions[11],
                state.Actions[12],
                state.Actions[13],
                state.Actions[14],
                state.Actions[15],
                state.Actions[16],
                //state.Actions[17], Wait 0.6 seconds
                //state.Actions[18], Set velocity
            ];
                state.AdjustTransitions("Grant Geo");
            }
            else if (self.FsmName == "Heart Container Control")
            {
                self.AddState("Grant Geo", () => { HeroController.instance.AddGeo(100); self.gameObject.SetActive(false); });
                FsmState state = self.GetState("Get");
                state.Actions =
                [
                    state.Actions[0],
                //state.Actions[1], Set invincibility
                state.Actions[2],
                //state.Actions[3], Set PD
                state.Actions[4],
                // state.Actions[5], Relinquish Control
                //state.Actions[6], Stop Animation
                //state.Actions[7], Play hp gain animation
                //state.Actions[8], Set gravity scale
                state.Actions[9], 
                //state.Actions[10], FSM CANCEL
                state.Actions[11],
                //state.Actions[12], Spawn Sprite
                //state.Actions[13], Spawn Sprite
                //state.Actions[14], Spawn Sprite
                state.Actions[15],
                state.Actions[16],
                state.Actions[17]
                //state.Actions[18], Wait
                //state.Actions[18], Set velocity
                ];
                state.AdjustTransitions("Grant Geo");
            }
            else if (self.gameObject.name == "Bretta Dazed" && self.FsmName == "Conversation Control")
            {
                self.AddState("Grant Geo", () =>
                {
                    HeroController.instance.AddGeo(250);
                    self.GetState("Idle").AdjustTransitions("Talk Finish");
                }, FsmTransitionData.FromTargetState("Talk Finish").WithEventName("FINISHED"));
                self.GetState("Idle").AdjustTransition("CONVO START", "Grant Geo");
            }
            else if (self.FsmName == "Control" && self.gameObject.name == "Shiny Item Acid")
            {
                self.GetState("Stop Push").AdjustTransitions("Finish");
                self.GetState("Regain Control").AddActions(() => HeroController.instance.AddGeo(50));
            }
            else if (self.FsmName == "Control" && self.gameObject.name == "Crystal Shaman")
                self.GetState("Init").AdjustTransitions("Broken");
            else if (self.FsmName == "Get Scream" && self.gameObject.name == "Scream Item")
                self.GetState("Check").AdjustTransitions("Destroy");
            else if (self.FsmName == "Door Control" && self.GetState("Idle") is FsmState state)
                state.ClearTransitions();
            else if (self.FsmName == "Control" && self.gameObject.name == "Cam Lock Control" && self.gameObject.scene.name == "Fungus3_40")
                self.GetState("Inactive").ClearTransitions();
            else if (self.FsmName == "Bench Control Spider")
            {
                // Spawn secret shiny.
                GameObject shiny = TreasureManager.SpawnShiny(TreasureType.RareOrb, new(64.04f, 113.4f), false);
                self.GetState("Sit Start").AddActions(() =>
                {
                    if (shiny != null)
                        GameObject.Destroy(shiny);
                });
            }
            // 64.04, 113.4
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify fsm: ", ex);
        }
        orig(self);
        try
        {
            if (self.gameObject.name == "Shaman Meeting")
                UnityEngine.Object.Destroy(self.gameObject);
            else if (self.gameObject.name == "Station Bell")
                UnityEngine.Object.Destroy(self.gameObject);
            else if (self.FsmName == "Challenge Start" && self.gameObject.name == "Challenge Prompt" && self.transform.parent != null && self.transform.parent.name == "Mantis Battle")
                self.gameObject.SetActive(false);
            else if (self.FsmName == "Control" && self.gameObject.name == "Hornet Fountain Encounter")
                UnityEngine.Object.Destroy(self.gameObject);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to disable fsm object: " + ex);
            throw;
        }
    }

    private static PersistentBoolData ForceSceneFlags(On.SceneData.orig_FindMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
    {
        if (persistentBoolData.sceneName == "Fungus1_21" && persistentBoolData.id == "Vine Platform (2)"
            || persistentBoolData.sceneName == "Fungus1_31" && persistentBoolData.id == "Toll Gate Machine"
            || persistentBoolData.sceneName == "Fungus1_13" && (persistentBoolData.id == "Vine Platform (2)" || persistentBoolData.id == "Vine Platform (1)")
            || persistentBoolData.sceneName == "Mines_33"
            || persistentBoolData.sceneName == "Fungus2_18" && persistentBoolData.id == "Mantis Lever"
            || persistentBoolData.sceneName == "Crossroads_ShamanTemple" && persistentBoolData.id == "Bone Gate"
            || persistentBoolData.sceneName == "Ruins1_05" && persistentBoolData.id == "Ruins Lever 3"
            || persistentBoolData.sceneName == "Mines_35" && persistentBoolData.id == "mine_1_quake_floor"
            || persistentBoolData.sceneName == "Deepnest_East_14" && persistentBoolData.id.Contains("Quake Floor")
            || persistentBoolData.sceneName == "Tutorial_01" && persistentBoolData.id == "Door"
            || persistentBoolData.sceneName == "Abyss_19" && persistentBoolData.id == "One Way Wall"
            || persistentBoolData.sceneName == "Deepnest_41" && persistentBoolData.id == "One Way Wall (2)"
            || persistentBoolData.sceneName == "Deepnest_01b" && persistentBoolData.id == "One Way Wall"
            || persistentBoolData.sceneName == "Deepnest_East_02" && persistentBoolData.id == "Quake Floor"
            || persistentBoolData.sceneName == "Mines_25" && persistentBoolData.id == "Quake Floor"
            || persistentBoolData.sceneName == "Ruins1_30" && persistentBoolData.id.Contains("Quake Floor Glass")
            || persistentBoolData.id == "Flamebearer Spawn"
            || persistentBoolData.sceneName == "Deepnest_Spider_Town" && persistentBoolData.id == "Collapser Small (12)")
        {
            persistentBoolData.activated = true;
            return persistentBoolData;
        }
        return orig(self, persistentBoolData);
    }

    private static void SkipSceneDataSave(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.Emit(OpCodes.Ret);
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == nameof(PlayerData.hasDreamNail) && (GameManager.instance.sceneName == "Mines_05"
            || GameManager.instance.sceneName == "Mines_11" || GameManager.instance.sceneName == "Mines_37"))
            return true;
        else if (name == nameof(PlayerData.crossroadsInfected))
            return CurrentRoomNumber >= CurrentRoomData.Count / 2;
        else if (name == nameof(PlayerData.spiderCapture))
            return false;
        return orig;
    }

    private static void PassHistoryData(HistoryData entry, RunResult result) => entry.FinalRoomNumber = CurrentRoomNumber;

    private static void OnEnemiesCleared()
    {
        FinishedEnemies = true;
        ClearExit();
    }

    public static void SceneAdjustments(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "GG_Engine")
            UnityEngine.Object.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
        else if (arg1.name == "Ruins1_05")
            SpawnTeleporter(new(3.68f, 153.19f), new(3.68f, 142.4f));
        else if (arg1.name == "Hive_01")
            SpawnTeleporter(new(44.54f, 11.09f), new(93.57f, 38.1f));
        else if (arg1.name == "Fungus3_48")
            SpawnTeleporter(new(19.2f, 105.6f), new(24.89f, 96f));
        else if (arg1.name == "Hive_03")
            SpawnTeleporter(new(47.33f, 142.4f), new(30.53f, 126.4f));
        else if (arg1.name == "Deepnest_East_14")
            SpawnTeleporter(new(141.57f, 58.15f), new(124.06f, 21.3f));
        else if (arg1.name == "Abyss_19")
            SpawnTeleporter(new(90.1f, 3.4f), new(114.41f, 3.4f));
    }

    #endregion
}
