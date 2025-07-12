using KorzUtils.Helper;
using Modding;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.UnityComponents.Other;
using TrialOfCrusaders.UnityComponents.StageElements;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Controls everything outside the actual runs inside the lobby.
/// </summary>
public class HubController : BaseController
{
    private int _rolledSeed;
    private List<SeedTablet> _seedTablets = [];

    internal GameObject Tink { get; set; }

    internal GameObject InspectPrefab { get; set; }

    public GameMode SelectedGameMode { get; set; }

    #region Setup

    public HubController()
    {
        PhaseManager.PhaseChanged += SetupStartRoom;
    }

    public override Phase[] GetActivePhases() => [Phase.Lobby];

    protected override void Enable()
    {
        LogManager.Log("Enable Hub Controller");
        On.PlayMakerFSM.OnEnable += FsmEdits;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneChanged;
        On.GameManager.BeginSceneTransition += ModifySceneTransition;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        On.HeroController.CanOpenInventory += BlockInventory;
    }

    protected override void Disable()
    {
        LogManager.Log("Disable Hub Controller");
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneChanged;
        On.GameManager.BeginSceneTransition -= ModifySceneTransition;
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        On.HeroController.CanOpenInventory -= BlockInventory;
    }

    #endregion

    #region Private Methods

    private void SetupTransitions()
    {
        try
        {
            List<TransitionPoint> transitions = [.. Object.FindObjectsOfType<TransitionPoint>()];
            TransitionPoint left = transitions.First(x => x.name == "left1");
            GameObject block = new("Block");
            block.SetActive(true);
            block.transform.position = new(14.5f, 9);
            block.AddComponent<BoxCollider2D>().size = new(1f, 8f);
            transitions.Remove(left);
            Object.Destroy(left);
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
                PDHelper.DreamOrbs = 0;
                PDHelper.HasDreamNail = true;
                PDHelper.MaxHealth = 5;
                PDHelper.SoulLimited = false;
                PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                GameCameras.instance.hudCanvas.gameObject.SetActive(false);
                PDHelper.Geo = 0;
                GameObject.Find("_GameCameras").transform.Find("HudCamera/Hud Canvas/Geo Counter").gameObject
                    .LocateMyFSM("Geo Counter")
                    .SendEvent("TO ZERO");
            }, true);
            GameObject.Destroy(GameObject.Find("Bronze Trial Board"));
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Failed to setup hub transitions.", ex);
        }
    }

    #endregion

    #region Event Handler

    /// <summary>
    /// Controls all FSM related modifications.
    /// </summary>
    private void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
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
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Failed to modify hub fsm edits.", ex);
        }
        orig(self);
    }

    private void ModifySceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
    {
        try
        {
            if (info.SceneName == "Dream_Backer_Shrine")
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
                StageRef.CurrentRoomData = SetupManager.GenerateRun(SelectedGameMode);
                StageRef.CurrentRoomIndex = -1;
                PhaseManager.TransitionTo(Phase.Run);
                // Grants mode specific items.
                if (SelectedGameMode == GameMode.Crusader)
                {
                    PDHelper.HasLantern = true;
                    PDHelper.HasSuperDash = true;
                    PDHelper.HasAcidArmour = true;
                }
            }
            else if (info.SceneName == "Room_Colosseum_02")
            {
                info.EntryGateName = "left1";
                info.SceneName = "Dream_Room_Believer_Shrine";
            }
            else if (info.SceneName.StartsWith("Room_Colosseum") && info.SceneName != "Room_Colosseum_01")
            {
                if (info.SceneName.Contains("Silver"))
                    SelectedGameMode = GameMode.Crusader;
                else
                    SelectedGameMode = GameMode.GrandCrusader;
                PDHelper.HasDreamNail = false;
                info.SceneName = "Deepnest_East_10";
                info.EntryGateName = "left1";
            }
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Error in hub modify scene transition.", ex);
        }
        orig(self, info);
    }

    private void SceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        try
        {
            if (arg1.name == "Room_Colosseum_01")
            {
                SetupTransitions();
                CoroutineHelper.WaitForHero(GameManager.instance.SaveGame, true);
            }
            else if (arg1.name == "Deepnest_East_10")
            {
                _seedTablets.Clear();
                Object.Destroy(GameObject.Find("plat_float_05 (1)"));
                Object.Destroy(GameObject.Find("plat_float_05"));
                Object.Destroy(GameObject.Find("white_ash_scenery_0004_5 (3)"));
                Object.Destroy(GameObject.Find("Inspect Region Ghost"));
                Object.Destroy(GameObject.Find("ghost_shrines_0001_markoth_corpse_01"));
                Object.Destroy(GameObject.Find("ghost_shrines_0002_markoth_corpse_02"));
                Object.Destroy(GameObject.Find("ghost_shrines_0003_markoth_corpse_03"));
                Object.Destroy(GameObject.Find("ghost_shrines_0003_markoth_corpse_03 (1)"));
                Object.Destroy(GameObject.Find("ghost_shrines_0003_markoth_corpse_03 (2)"));
                _rolledSeed = Random.Range(100000000, 1000000000);
                // Prevent special seed from appearing.
                while (_rolledSeed == 777777777)
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
                    obstacleGameObject.transform.position = new(xPosition, yPosition, 0.02f);
                    obstacleGameObject.transform.localScale = new(2f, 2f, 1f);
                    obstacleGameObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Mirror");
                    obstacleGameObject.AddComponent<BoxCollider2D>().size = new(1f, 1f);
                    obstacleGameObject.GetComponent<BoxCollider2D>().isTrigger = true;
                    obstacleGameObject.AddComponent<TinkEffect>().blockEffect = Tink;
                    obstacleGameObject.layer = 11;
                    SeedTablet tablet = obstacleGameObject.AddComponent<SeedTablet>();
                    _seedTablets.Add(tablet);
                    tablet.Index = i;
                    tablet.InitialNumber = int.Parse("" + _rolledSeed.ToString()[i]);
                    tablet.Number = tablet.InitialNumber;

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

                GameObject explanationSprite = new("Tablet Sprite");
                explanationSprite.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Sign");
                explanationSprite.transform.position = new(11.95f, 4.5f, 0.01f);
                explanationSprite.transform.localScale = new(2f, 2f);
                GameObject explanationTablet = GameObject.Instantiate(InspectPrefab);
                explanationTablet.SetActive(true);
                explanationTablet.LocateMyFSM("inspect_region").FsmVariables.FindFsmString("Game Text Convo").Value = "Explanation_Trial";
                explanationTablet.transform.position = new(11.95f, 4.4f);
                CoroutineHelper.WaitForHero(() =>
                {
                    GameObject startTransition = new("Start Transition");
                    SpecialTransition transition = startTransition.AddComponent<SpecialTransition>();
                    transition.VanillaTransition = GameObject.Find("left1").GetComponent<TransitionPoint>();
                    transition.LoadIntoDream = true;
                }, true);
            }
        }
        catch (System.Exception ex)
        {
            LogManager.Log("Error on hub scene changed.", ex);
        }
    }

    private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "Explanation_Trial")
        {
            if (SelectedGameMode == GameMode.GrandCrusader)
                return $"{LobbyDialog.ExplanationTrialGrandCrusader}<page>{LobbyDialog.SeededTutorial}";
            else
                return $"{LobbyDialog.ExplanationTrialCrusader}<page>{LobbyDialog.SeededTutorial}";
        }
        else if (key == "TRIAL_BOARD_SILVER")
            return "Begin Trial of the Crusader?";
        else if (key == "TRIAL_BOARD_GOLD")
            return "Begin Trial of the Grand Crusader?";
        else if (key == "LITTLE_FOOL_UNPAID")
            return "This trial is not available at the moment.";
        else if (key == "LITTLE_FOOL_DREAM")
            return LittleFoolDialog.FoolDream;
        return orig;
    }

    private bool BlockInventory(On.HeroController.orig_CanOpenInventory orig, HeroController self)
    {
        orig(self);
        return false;
    }

    private void SetupStartRoom(Phase currentPhase, Phase newPhase)
    {
        if (currentPhase == Phase.Lobby && newPhase == Phase.Run)
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
                if (RngManager.Seed == 000000206)
                    TreasureManager.SpawnShiny(TreasureType.RareOrb, new(109f, 20.4f), false);
                else
                    TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(109f, 20.4f), false);

                if (RngManager.Seed == 000001225)
                    TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(114f, 20.4f), false);
            }, () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GG_Spa", true);
    }

    #endregion
}
