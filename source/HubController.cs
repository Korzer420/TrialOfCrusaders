using KorzUtils.Helper;
using System.Collections.Generic;
using System.Linq;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.UnityComponents;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders;

/// <summary>
/// Controls everything outside the actual runs inside the lobby.
/// </summary>
internal static class HubController
{
    private static int _rolledSeed;
    private static List<SeedTablet> _seedTablets = [];

    internal static void Initialize()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
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
            if (finalSeed != _rolledSeed)
            {
                // ToDo: Flag for seeded run.
            }
            StageController.CurrentRoomData = SetupController.GenerateRun(finalSeed);
            StageController.CurrentRoomIndex = -1;
            StageController.Initialize();
            CoroutineHelper.WaitUntil(() => 
            {
                GameObject pedestal = new("Pedestal");
                pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Pedestal");
                pedestal.transform.position = new(104.68f, 15.4f, 0);
                pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
                pedestal.layer = 8; // Terrain layer
                pedestal.SetActive(true);

                pedestal = new("Pedestal2");
                pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Pedestal");
                pedestal.transform.position = new(109f, 15.4f, 0);
                pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
                pedestal.layer = 8; // Terrain layer
                pedestal.SetActive(true);
                // Spawn two orbs at the start.
                TreasureController.SpawnShiny(TreasureType.PrismaticOrb, new(104.68f, 20.4f), false);
                TreasureController.SpawnShiny(TreasureType.NormalOrb, new(109f, 20.4f), false);
                Unload();
            },() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GG_Spa", true);
        }
        orig(self, info);
    }

    private static void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (arg1.name == "Room_Colosseum_01")
            SetupTransitions();
        else if (arg1.name == "Deepnest_East_10")
        {
            GameObject.Destroy(GameObject.Find("plat_float_05 (1)"));
            GameObject.Destroy(GameObject.Find("plat_float_05"));
            GameObject.Destroy(GameObject.Find("white_ash_scenery_0004_5 (3)"));
            GameObject.Destroy(GameObject.Find("Inspect Region Ghost"));
            _rolledSeed = Random.Range(100000000, 1000000000);
            float xPosition = 18.2f;
            float yPosition = 9.5f;
            for (int i = 0; i < 9; i++)
            {
                // 18.2f
                // 21.7f, 9.5f
                // 25.2f, 9.5f
                // 4.9f oder 9.5f Y
                GameObject obstacleGameObject = new("Sign");
                obstacleGameObject.SetActive(false);
                obstacleGameObject.transform.position = new(xPosition, yPosition, -0.08f);
                obstacleGameObject.transform.localScale = new(2f, 2f, 1f);
                obstacleGameObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Tablet");
                obstacleGameObject.AddComponent<BoxCollider2D>().size = new(1f, 1f);
                obstacleGameObject.GetComponent<BoxCollider2D>().isTrigger = true;
                obstacleGameObject.AddComponent<TinkEffect>().blockEffect = ConcussionEffect.ConcussionObject.GetComponent<TinkEffect>().blockEffect;
                obstacleGameObject.layer = 11;
                SeedTablet tablet = obstacleGameObject.AddComponent<SeedTablet>();
                _seedTablets.Add(tablet);
                tablet.Index = i;
                tablet.Number = int.Parse(""+_rolledSeed.ToString()[i]);

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
        else if (arg1.name == "GG_Spa")
        {
            CoroutineHelper.WaitForHero(() =>
            {
                GameObject startTransition = new("Start Transition");
                SpecialTransition transition = startTransition.AddComponent<SpecialTransition>();
                transition.VanillaTransition = GameObject.Find("right1").GetComponent<TransitionPoint>();
                transition.LoadIntoDream = false;
            }, true);
        }
    }

    internal static void Unload()
    {
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        On.GameManager.BeginSceneTransition -= GameManager_BeginSceneTransition;
    }

    private static void SetupTransitions()
    {
        List<TransitionPoint> transitions = [.. GameObject.FindObjectsOfType<TransitionPoint>()];
        TransitionPoint left = transitions.First(x => x.name == "left1");
        GameObject block = new("Block");
        block.SetActive(true);
        block.AddComponent<BoxCollider2D>().size = left.GetComponent<BoxCollider2D>().size;
        transitions.Remove(left);
        Component.Destroy(left);
        transitions.First(x => x.name == "bot1").targetScene = "Dream_Room_Believer_Shrine";
    }

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
}
