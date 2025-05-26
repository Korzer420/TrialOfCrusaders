using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

internal static class StageController
{
    private static bool _enabled;
    private static List<TransitionPoint> _transitionPoints = [];
    private static List<SpecialTransition> _specialTransitions = [];
    private static int _treasureRoomCooldown = 0;
    private static (string, string) _intendedDestination = new();

    public static bool QuietRoom { get; set; } = true;

    public static bool UpcomingTreasureRoom { get; set; }

    public static int CurrentRoomIndex { get; set; } = 0;

    public static int CurrentRoomNumber => CurrentRoomIndex + 1;

    public static List<RoomData> CurrentRoomData { get; set; } = [];

    public static GameObject TransitionObject { get; set; }

    public static bool FinishedEnemies { get; set; }

    public static RoomData CurrentRoom => CurrentRoomIndex == -1 ? null : CurrentRoomData[CurrentRoomIndex];

    public static event Action<bool> RoomEnded;

    internal static void Initialize()
    {
        if (_enabled)
            return;
        LogHelper.Write<TrialOfCrusaders>("Enable Stage Controller", KorzUtils.Enums.LogType.Debug);
        QuietRoom = true;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        // "Remove" all benches.
        On.RestBench.Start += RestBench_Start;
        // These hooks ensure that nothing is saved within a scene so it can repeated indefinetly.
        On.SceneData.SaveMyState_GeoRockData += SceneData_SaveMyState_GeoRockData;
        On.SceneData.SaveMyState_PersistentBoolData += SceneData_SaveMyState_PersistentBoolData;
        On.SceneData.SaveMyState_PersistentIntData += SceneData_SaveMyState_PersistentIntData;
        // Change flags to prevent softlocks (like already lowering platforms)
        On.SceneData.FindMyState_PersistentBoolData += SceneData_FindMyState_PersistentBoolData;

        // Transition handling.
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
        On.TransitionPoint.Start += TransitionPoint_Start;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += Block_Doors;
        CombatController.EnemiesCleared += CombatController_EnemiesCleared;
        HistoryController.CreateEntry += HistoryController_CreateEntry;
        
        _enabled = true;
    }

    internal static void Unload()
    {
        if (!_enabled)
            return;
        LogHelper.Write<TrialOfCrusaders>("Disable Stage Controller", KorzUtils.Enums.LogType.Debug);
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        // "Remove" all benches.
        On.RestBench.Start -= RestBench_Start;
        // These hooks ensure that nothing is saved within a scene so it can repeated indefinetly.
        On.SceneData.SaveMyState_GeoRockData -= SceneData_SaveMyState_GeoRockData;
        On.SceneData.SaveMyState_PersistentBoolData -= SceneData_SaveMyState_PersistentBoolData;
        On.SceneData.SaveMyState_PersistentIntData -= SceneData_SaveMyState_PersistentIntData;
        // Change flags to prevent softlocks (like already lowering platforms)
        On.SceneData.FindMyState_PersistentBoolData -= SceneData_FindMyState_PersistentBoolData;

        // Transition handling.
        On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
        On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;
        On.TransitionPoint.Start -= TransitionPoint_Start;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter -= Block_Doors;
        CombatController.EnemiesCleared -= CombatController_EnemiesCleared;
        HistoryController.CreateEntry -= HistoryController_CreateEntry;
        
        // Reset data.
        CurrentRoomIndex = -1;
        QuietRoom = true;
        _transitionPoints.Clear();
        _specialTransitions.Clear();
        CurrentRoomData.Clear();
        FinishedEnemies = false;
        _enabled = false;
    }

    private static void CombatController_EnemiesCleared()
    {
        FinishedEnemies = true;
        ClearExit();
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "GG_Engine")
            UnityEngine.Object.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
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
        _specialTransitions.First().GetComponent<BoxCollider2D>().isTrigger = true;
        _specialTransitions.First().WaitForItem = false;
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
        source.PlayOneShot(SoundHelper.CreateSound(ResourceHelper.LoadResource<TrialOfCrusaders>(room ? "SoundEffects.Room_Clear.wav" : "SoundEffects.Run_Finished.wav"), "Room_Clear"), 2f);
        TrialOfCrusaders.Holder.StartCoroutine(CoroutineHelper.WaitUntil(() => UnityEngine.Object.Destroy(audioObject), () => source == null || !source.isPlaying));
    }

    internal static List<RoomData> LoadRoomData() => ResourceHelper.LoadJsonResource<TrialOfCrusaders, List<RoomData>>("Data.RoomData.json");

    private static void HistoryController_CreateEntry(HistoryData entry, RunResult result) => entry.FinalRoomNumber = CurrentRoomNumber;

    private static void Block_Doors(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
    {
        if (self.IsCorrectContext("Door Control", null, "Can Enter?"))
            self.boolVariable.Value = self.boolVariable.Value && FinishedEnemies && _specialTransitions.Count == 0;
        orig(self);
    }

    #region Transition Handling

    private static void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
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
                RoomEnded?.Invoke(QuietRoom);
                FinishedEnemies = false;
                // Check for ending.
                if (CurrentRoomIndex == 119)
                { 
                    CurrentRoomIndex++;
                    QuietRoom = true;
                    info.EntryGateName = "left1";
                    info.SceneName = "Room_Colosseum_Bronze";
                    HistoryController.AddEntry(RunResult.Completed);
                    Unload();
                    CombatController.Unload();
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
                    }
                    else
                    {
                        CurrentRoomIndex++;
                        QuietRoom = CurrentRoomData[CurrentRoomIndex].IsQuietRoom;
                        if (QuietRoom)
                            info.SceneName = "GG_Engine";
                        else
                            info.SceneName = CurrentRoomData[CurrentRoomIndex].Name;
                        //Block Mage Knight at 16.32f, 74.4f
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
                    // Not between 37-43 and 77-83 (40 and 80 could be rare treasure rooms if the spells there have been obtained already.)
                    if (_treasureRoomCooldown == 0 && !UpcomingTreasureRoom && CurrentRoomNumber >= 10 && CurrentRoomNumber <= 115 && (CurrentRoomNumber <= 37 || CurrentRoomNumber >= 43) && (CurrentRoomNumber <= 77 || CurrentRoomNumber >= 83)
                        && !CurrentRoomData[CurrentRoomIndex - 1].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom)
                    {
                        float chance = 0.5f;
                        if (CombatController.HasPower<Damocles>(out _))
                            chance += 4.5f;
                        if (CombatController.HasPower<TreasureHunter>(out _))
                            chance += 2.5f;
                        UpcomingTreasureRoom = chance >= RngProvider.GetStageRandom(1f, 100f);
                        if (UpcomingTreasureRoom)
                            _treasureRoomCooldown = 6;
                        else
                            _treasureRoomCooldown = _treasureRoomCooldown.Lower(_treasureRoomCooldown);
                    }
                    else
                        UpcomingTreasureRoom = false;
                }
                _intendedDestination = new(info.SceneName, info.EntryGateName);
            }
        }
        orig(self, info);
    }

    private static void TransitionPoint_Start(On.TransitionPoint.orig_Start orig, TransitionPoint self)
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
        if (QuietRoom || UpcomingTreasureRoom || CurrentRoomIndex < 119 && (CurrentRoomData[CurrentRoomIndex + 1].BossRoom || CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom))
        {
            if (self.isADoor)
                return;
            GameObject transitionObject = new("Trial Transition");
            SpecialTransition transition = transitionObject.AddComponent<SpecialTransition>();
            transition.LoadIntoDream = CurrentRoomIndex < 119 && CurrentRoomData[CurrentRoomIndex + 1].BossRoom || CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom;
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
                TreasureManager.SpawnShiny(RngProvider.GetStageRandom(0, 100) < 10 ? TreasureType.RareOrb : TreasureType.NormalOrb, new(94.23f, 16.4f), false);
            else
            {
                // Special case for spells at 40 and 80. If they have been obtained already, they get replaced by a rare treasure guaranteed.
                if (CurrentRoomNumber == 40 || CurrentRoomNumber == 80)
                {
                    TreasureType intendedSpell = (TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name);
                    if (intendedSpell == TreasureType.Fireball && PDHelper.FireballLevel != 0 || intendedSpell == TreasureType.Quake && PDHelper.QuakeLevel != 0)
                    {
                        TreasureManager.SpawnShiny(TreasureType.RareOrb, new(94.23f, 16.4f), false);
                        return;
                    }
                }
                TreasureManager.SpawnShiny((TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name), new(94.23f, 16.4f), false);
                _specialTransitions.Last().WaitForItem = true;
            }
        }
    }

    private static void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        //if (Spawner.ContinueSpawn)
        //    return;
        _intendedDestination.Item1 = null;
        if (!QuietRoom)
            PlayMakerFSM.BroadcastEvent("DREAM GATE CLOSE");
    }

    #endregion

    private static void RestBench_Start(On.RestBench.orig_Start orig, RestBench self)
    {
        orig(self);
        // To ensure other mods are running correctly, we just move the bench out of bounce.
        if (GameManager.instance?.RespawningHero != true)
            self.transform.position = new(-4000f, -4000f);
    }

    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
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
        orig(self);
        if (self.gameObject.name == "Shaman Meeting")
            UnityEngine.Object.Destroy(self.gameObject);
        else if (self.gameObject.name == "Station Bell")
            UnityEngine.Object.Destroy(self.gameObject);
    }

    internal static IEnumerator WaitForTransition()
    {
        HeroController.instance.RelinquishControl();
        // Todo: Extra checks for radiance (?).
        float passedTime = 0f;
        while(passedTime < 2)
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

    private static PersistentBoolData SceneData_FindMyState_PersistentBoolData(On.SceneData.orig_FindMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
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
            || persistentBoolData.sceneName == "Tutorial_01" && persistentBoolData.id == "Door")
        {
            persistentBoolData.activated = true;
            return persistentBoolData;
        }
        return orig(self, persistentBoolData);
    }

    private static void SceneData_SaveMyState_PersistentIntData(On.SceneData.orig_SaveMyState_PersistentIntData orig, SceneData self, PersistentIntData persistentIntData) { }

    private static void SceneData_SaveMyState_PersistentBoolData(On.SceneData.orig_SaveMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData) { }

    private static void SceneData_SaveMyState_GeoRockData(On.SceneData.orig_SaveMyState_GeoRockData orig, SceneData self, GeoRockData geoRockData) { }
}
