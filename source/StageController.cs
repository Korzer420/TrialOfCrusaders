using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;
using UnityEngine.Audio;

namespace TrialOfCrusaders;

internal static class StageController
{
    private static List<TransitionPoint> _transitionPoints = [];
    private static List<SpecialTransition> _specialTransitions = [];

    public static bool QuietRoom { get; set; } = true;

    public static bool UpcomingTreasureRoom { get; set; }

    public static List<HealthManager> Enemies { get; set; } = [];

    public static int CurrentRoomIndex { get; set; } = 0;

    public static int CurrentRoomNumber => CurrentRoomIndex + 1; 

    public static List<RoomData> CurrentRoomData { get; set; } = [];

    public static List<RoomData> RoomList { get; set; } = [];

    public static GameObject TransitionObject { get; set; }

    public static List<Func<float, float>> CalculateTreasureRoom = [];

    private static (string, string) _intendedDestination = new();

    static StageController() => RoomList = ResourceHelper.LoadJsonResource<TrialOfCrusaders, List<RoomData>>("Data.RoomData.json");

    internal static void Initialize()
    {
        QuietRoom = true;
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        // "Remove" all benches.
        On.RestBench.Start += RestBench_Start;
        On.SceneData.FindMyState_PersistentBoolData += SceneData_FindMyState_PersistentBoolData;
        // These hooks ensure that nothing is saved within a scene so it can repeated indefinetly.
        On.SceneData.SaveMyState_GeoRockData += SceneData_SaveMyState_GeoRockData;
        On.SceneData.SaveMyState_PersistentBoolData += SceneData_SaveMyState_PersistentBoolData;
        On.SceneData.SaveMyState_PersistentIntData += SceneData_SaveMyState_PersistentIntData;
        ModHooks.OnEnableEnemyHook += ModHooks_OnEnableEnemyHook;
        On.HealthManager.OnEnable += HealthManager_OnEnable;
        On.HealthManager.Die += HealthManager_Die;
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
        On.TransitionPoint.Start += TransitionPoint_Start;
        On.HutongGames.PlayMaker.Actions.BoolTest.OnEnter += BoolTest_OnEnter;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "GG_Engine")
            GameObject.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
    }

    internal static void EnableExit() 
    {
        _specialTransitions.First().GetComponent<BoxCollider2D>().isTrigger = true;
        _specialTransitions.First().WaitForItem = false;
    }

    public static void PlayClearSound()
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
        source.PlayOneShot(SoundHelper.CreateSound(ResourceHelper.LoadResource<TrialOfCrusaders>("SoundEffects.Room_Clear.wav"), "Room_Clear"), 2f);
        TrialOfCrusaders.Holder.StartCoroutine(CoroutineHelper.WaitUntil(() => GameObject.Destroy(audioObject), () => !source.isPlaying));
    }

    private static bool ModHooks_OnEnableEnemyHook(GameObject enemy, bool isAlreadyDead) => false;

    private static void BoolTest_OnEnter(On.HutongGames.PlayMaker.Actions.BoolTest.orig_OnEnter orig, BoolTest self)
    {
        if (self.IsCorrectContext("Door Control", null, "Can Enter?"))
            self.boolVariable.Value = self.boolVariable.Value && Enemies.Count == 0;
        orig(self);
    }

    #region Transition Handling

    private static void TransitionPoint_Start(On.TransitionPoint.orig_Start orig, TransitionPoint self)
    {
        orig(self);
        _transitionPoints.Add(self);
        if (!QuietRoom)
        {
            Gate gate = self.gameObject.AddComponent<Gate>();
            CoroutineHelper.WaitForHero(() => gate.PlaceCollider(), true);
        }
        if (QuietRoom || UpcomingTreasureRoom || (CurrentRoomIndex < 119 && (CurrentRoomData[CurrentRoomIndex + 1].BossRoom || CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom)))
        {
            if (self.isADoor)
                return;
            GameObject transitionObject = new("Trial Transition");
            SpecialTransition transition = transitionObject.AddComponent<SpecialTransition>();
            transition.LoadIntoDream = false;
            transition.VanillaTransition = self;
            _specialTransitions.Add(transition);
        }
        if (QuietRoom && CurrentRoomIndex != -1 && self.name == "right1")
        {
            GameObject pedestal = new("Pedestal");
            pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Pedestal");
            pedestal.transform.position = new(94.23f, 14.8f, -0.1f);
            pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
            pedestal.layer = 8; // Terrain layer
            pedestal.SetActive(true);
            // If we are in a quiet room even though the room flag isn't set, we are in a treasure room instead.
            if (!CurrentRoomData[CurrentRoomIndex].IsQuietRoom)
                TreasureController.SpawnShiny(RngProvider.GetStageRandom(0, 100) < 10 ? TreasureType.RareOrb : TreasureType.NormalOrb, new(94.23f, 16.4f), false);
            else
            {
                // Special case for spells at 40 and 80. If they have been obtained already, they get replaced by a rare treasure guaranteed.
                if (CurrentRoomNumber == 40 || CurrentRoomNumber == 80)
                {
                    TreasureType intendedSpell = (TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name);
                    if ((intendedSpell == TreasureType.Fireball && PDHelper.FireballLevel != 0) || (intendedSpell == TreasureType.Quake && PDHelper.QuakeLevel != 0))
                    {
                        TreasureController.SpawnShiny(TreasureType.RareOrb, new(94.23f, 16.4f), false);
                        return;
                    }
                }
                TreasureController.SpawnShiny((TreasureType)Enum.Parse(typeof(TreasureType), CurrentRoomData[CurrentRoomIndex].Name), new(94.23f, 16.4f), false);
                _specialTransitions.Last().WaitForItem = true;
            }
        }
    }

    private static void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        // Knight/Effect/NA Charged
        // GG_Engine 93f, 15.4f
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
                info.PreventCameraFadeOut = QuietRoom;
                GameManager.instance.cameraCtrl.gameObject.LocateMyFSM("CameraFade").FsmVariables.FindFsmBool("No Fade").Value = QuietRoom;

                // Treasure rooms can only appear under these conditions:
                // Not later than 115.
                // Not earlier than 10.
                // Not after/before a quiet room.
                // Not between 37-43 and 77-83 (40 and 80 could be rare treasure rooms if the spells there have been obtained already.)
                if (!UpcomingTreasureRoom && (CurrentRoomNumber >= 10 && CurrentRoomNumber <= 115 && (CurrentRoomNumber <= 37 || CurrentRoomNumber >= 43) && (CurrentRoomNumber <= 77 || CurrentRoomNumber >= 83)
                    && !CurrentRoomData[CurrentRoomIndex - 1].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex].IsQuietRoom && !CurrentRoomData[CurrentRoomIndex + 1].IsQuietRoom))
                {
                    float chance = 0.5f;
                    foreach (Func<float, float> handler in CalculateTreasureRoom)
                        chance = handler.Invoke(chance);
                    UpcomingTreasureRoom = chance >= RngProvider.GetStageRandom(1f, 100f);
                }
                else
                    UpcomingTreasureRoom = false;
                _intendedDestination = new(info.SceneName, info.EntryGateName);
            }
        }
        else
        {
            // ToDo: Unload if return to menu.
        }
        orig(self, info);
    }

    private static void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        _intendedDestination.Item1 = null;
        Enemies ??= [];
        List<HealthManager> newEnemies = [];
        // To do: Mark initial enemies (for reward effects)
        foreach (HealthManager item in Enemies)
            if (item != null && item.gameObject != null && item.gameObject.scene != null && item.gameObject.scene.name == GameManager.instance.sceneName)
                newEnemies.Add(item);
        if (!QuietRoom)
            PlayMakerFSM.BroadcastEvent("DREAM GATE CLOSE");
        Enemies = newEnemies;
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
            GameObject.Destroy(self.gameObject);
        else if (self.gameObject.name == "Station Bell")
            GameObject.Destroy(self.gameObject);
    }

    private static void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        bool contained = Enemies.Contains(self);
        Enemies.Remove(self);
        // Unity doesn't like the "?" operator.
        for (int i = 0; i < Enemies.Count; i++)
            if (Enemies[i] == null || Enemies[i].gameObject == null || !Enemies[i].gameObject.activeSelf)
            {
                Enemies.RemoveAt(i);
                i--;
            }
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (Enemies.Count == 0 && !QuietRoom && contained)
        {
            if (CurrentRoomData[CurrentRoomIndex].BossRoom && contained)
                TrialOfCrusaders.Holder.StartCoroutine(WaitForTransition());
            else
            {
                foreach (TransitionPoint point in _transitionPoints)
                    point.gameObject.GetComponent<Gate>()?.Revert();
                foreach (SpecialTransition item in _specialTransitions)
                    item.GetComponent<BoxCollider2D>().isTrigger = true;
                PlayClearSound();
            }
        }
    }

    private static void HealthManager_OnEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
    {
        // Prevent "immortal" enemies.
        if (self.hp != 9999 && self.gameObject.name != "Mender Bug" && !self.gameObject.name.Contains("Pigeon") && !self.gameObject.name.Contains("Hatcher Baby Spawner"))
        {
            Enemies.Add(self);
            LogHelper.Write("Added enemy: " + Enemies.Last().gameObject.name);
        }
        orig(self);
    }

    private static IEnumerator WaitForTransition()
    {
        // Todo: Extra checks for collector and radiance (?).
        yield return new WaitForSeconds(5f);
        GameObject transition = new("Trial Transition Starter");
        SpecialTransition specialTransition = transition.AddComponent<SpecialTransition>();
        specialTransition.LoadIntoDream = false;
        transition.transform.position = new(-5000, -5000f);
        specialTransition.StartGodhomeTransition();
    }

    private static PersistentBoolData SceneData_FindMyState_PersistentBoolData(On.SceneData.orig_FindMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
    {
        if ((persistentBoolData.sceneName == "Fungus1_21" && persistentBoolData.id == "Vine Platform (2)")
            || (persistentBoolData.sceneName == "Fungus1_31" && persistentBoolData.id == "Toll Gate Machine")
            || (persistentBoolData.sceneName == "Fungus1_13" && (persistentBoolData.id == "Vine Platform (2)" || persistentBoolData.id == "Vine Platform (1)"))
            || persistentBoolData.sceneName == "Mines_33"
            || (persistentBoolData.sceneName == "Fungus2_18" && persistentBoolData.id == "Mantis Lever")
            || (persistentBoolData.sceneName == "Crossroads_ShamanTemple" && persistentBoolData.id == "Bone Gate")
            || (persistentBoolData.sceneName == "Ruins1_05" && persistentBoolData.id == "Ruins Lever 3")
            || (persistentBoolData.sceneName == "Mines_35" && persistentBoolData.id == "mine_1_quake_floor")
            || (persistentBoolData.sceneName == "Deepnest_East_14" && persistentBoolData.id.Contains("Quake Floor"))
            || (persistentBoolData.sceneName == "Tutorial_01" && persistentBoolData.id == "Door"))
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
