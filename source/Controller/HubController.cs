using KorzUtils.Helper;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.UnityComponents;
using TrialOfCrusaders.UnityComponents.Debuffs;
using TrialOfCrusaders.UnityComponents.StageElements;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Controls everything outside the actual runs inside the lobby.
/// </summary>
internal static class HubController
{
    private static bool _enabled;
    private static int _rolledSeed;
    private static List<SeedTablet> _seedTablets = [];

    internal static GameObject Tink { get; set; }

    #region Setup

    internal static void Initialize()
    {
        if (_enabled)
            return;
        LogHelper.Write<TrialOfCrusaders>("Enable Hub Controller", KorzUtils.Enums.LogType.Debug);
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;

        _enabled = true;
    }

    internal static void Unload()
    {
        if (!_enabled)
            return;
        LogHelper.Write<TrialOfCrusaders>("Disable Hub Controller", KorzUtils.Enums.LogType.Debug);
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
        _enabled = false;
    }

    #endregion

    #region Private Methods

    private static void SetupTransitions()
    {
        List<TransitionPoint> transitions = [.. Object.FindObjectsOfType<TransitionPoint>()];
        TransitionPoint left = transitions.First(x => x.name == "left1");
        GameObject block = new("Block");
        block.SetActive(true);
        block.transform.position = new(14.5f, 9);
        block.AddComponent<BoxCollider2D>().size = new(1f, 8f);
        transitions.Remove(left);
        Object.Destroy(left);
        transitions.First(x => x.name == "bot1").targetScene = "Dream_Room_Believer_Shrine";
        CoroutineHelper.WaitForHero(() =>
        {
            // Reset data
            PDHelper.HasDash = false;
            PDHelper.CanDash = false;
            PDHelper.HasShadowDash = false;
            PDHelper.HasWalljump = false;
            PDHelper.CanWallJump = false;
            PDHelper.HasDoubleJump = false;
            PDHelper.HasLantern = false;
            PDHelper.HasSuperDash = false;
            PDHelper.HasAcidArmour = false;
            PDHelper.FireballLevel = 0;
            PDHelper.QuakeLevel = 0;
            PDHelper.Geo = 0;
            PDHelper.DreamOrbs = 0;
            PDHelper.HasDreamNail = true;
            PDHelper.MaxHealth = 5;
            PDHelper.SoulLimited = false;
            PDHelper.GrimmChildLevel = 0;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(true);
            PDHelper.Geo = 0;
            GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas/Geo Counter").gameObject
                .LocateMyFSM("Geo Counter")
                .SendEvent("TO ZERO");
            GameManager.instance.SaveGame();
        }, true);
    } 

    #endregion

    #region Event Handler

    /// <summary>
    /// Controls all FSM related modifications.
    /// </summary>
    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        if (self.gameObject.name == "Little Fool NPC" && self.FsmName == "Conversation Control")
        {
            self.GetState("Opened?").AdjustTransition("FINISHED", "Gate Opened");
            self.GetState("Gate Opened").InsertActions(0, () =>
            {
                // Select dialog.
                GameHelper.OneTimeMessage("LITTLE_FOOL_CHALLENGE", LittleFoolDialog.Greeting, "Minor NPC");
            });
        }
        orig(self);
    }

    private static void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        if (info.SceneName == "Dream_Room_Believer_Shrine")
            info.EntryGateName = "left1";
        else if (info.SceneName == "Dream_Backer_Shrine")
        {
            info.SceneName = "Room_Colosseum_01";
            info.EntryGateName = "bot1";
        }
        else if (info.SceneName == "Select Target")
        {
            info.SceneName = "GG_Spa";
            info.EntryGateName = "door_dreamEnter";
            int finalSeed = int.Parse(string.Join("", _seedTablets.Select(x => x.Number.ToString())));
            RngManager.Seeded = finalSeed != _rolledSeed;
            RngManager.Seed = finalSeed;
            StageController.CurrentRoomData = SetupManager.GenerateNormalRun();
            StageController.CurrentRoomIndex = -1;
            PhaseController.TransitionTo(Phase.Run);
        }
        else if (info.SceneName == "Room_Colosseum_Bronze")
        {
            PDHelper.HasDreamNail = false;
            info.SceneName = "Deepnest_East_10";
            info.EntryGateName = "left1";
        }
        orig(self, info);
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "Room_Colosseum_01")
            SetupTransitions();
        else if (arg1.name == "Deepnest_East_10")
        {
            _seedTablets.Clear();
            Object.Destroy(GameObject.Find("plat_float_05 (1)"));
            Object.Destroy(GameObject.Find("plat_float_05"));
            Object.Destroy(GameObject.Find("white_ash_scenery_0004_5 (3)"));
            Object.Destroy(GameObject.Find("Inspect Region Ghost"));
            _rolledSeed = Random.Range(100000000, 1000000000);
            float xPosition = 18.2f;
            float yPosition = 9.5f;
            for (int i = 0; i < 9; i++)
            {
                // 18.2f
                // 21.7f, 9.5f
                // 25.2f, 9.5f
                // 4.9f or 9.5f Y
                GameObject obstacleGameObject = new("Sign");
                obstacleGameObject.SetActive(false);
                obstacleGameObject.transform.position = new(xPosition, yPosition, -0.08f);
                obstacleGameObject.transform.localScale = new(2f, 2f, 1f);
                obstacleGameObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Mirror");
                obstacleGameObject.AddComponent<BoxCollider2D>().size = new(1f, 1f);
                obstacleGameObject.GetComponent<BoxCollider2D>().isTrigger = true;
                obstacleGameObject.AddComponent<TinkEffect>().blockEffect = Tink;
                obstacleGameObject.layer = 11;
                SeedTablet tablet = obstacleGameObject.AddComponent<SeedTablet>();
                _seedTablets.Add(tablet);
                tablet.Index = i;
                tablet.Number = int.Parse("" + _rolledSeed.ToString()[i]);

                GameObject abilitySprite = new("Ability Sprite");
                abilitySprite.transform.SetParent(obstacleGameObject.transform);
                abilitySprite.transform.localPosition = new(0f, -0.1f, -0.01f);
                abilitySprite.transform.localScale = new(.28f, .28f);
                abilitySprite.AddComponent<SpriteRenderer>();
                abilitySprite.SetActive(true);
                obstacleGameObject.SetActive(true);
                xPosition += 3.5f;
                if (xPosition == 25.2f && yPosition == 4.9f)
                    xPosition += 3.5f;
                else if (xPosition > 32.5)
                {
                    yPosition -= 4.6f;
                    xPosition = 18.2f;
                }
            }
            CoroutineHelper.WaitForHero(() =>
            {
                GameObject startTransition = new("Start Transition");
                SpecialTransition transition = startTransition.AddComponent<SpecialTransition>();
                transition.VanillaTransition = GameObject.Find("left1").GetComponent<TransitionPoint>();
                transition.LoadIntoDream = true;
            }, true);
        }
    } 

    #endregion
}
