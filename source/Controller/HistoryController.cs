using InControl;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.SaveData;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

internal static class HistoryController
{
    private static bool _active;
    private static int _pageIndex = 0;
    private static int _powerPageIndex = 0;
    private static Dictionary<string, (SpriteRenderer, TextMeshPro)> _elementLookUp = [];
    private static readonly List<string> _seedSprites =
    [
        "CrystalDash",
        "CycloneSlash",
        "DesolateDive",
        "GreatSlash",
        "HowlingWraiths",
        "IsmasTear",
        "MantisClaw",
        "MonarchWings",
        "MothwingCloak",
        "VengefulSpirit"
    ];
    private static readonly string[] _tabletKeys =
    [
        "BELIEVE_TAB_07",
        "BELIEVE_TAB_24",
        "BELIEVE_TAB_08",
        "BELIEVE_TAB_36",
        "BELIEVE_TAB_05",
        "BELIEVE_TAB_01",
        "BELIEVE_TAB_35",
        "BELIEVE_TAB_57",
        "BELIEVE_TAB_03",
        "BELIEVE_TAB_06",
        "BELIEVE_TAB_09",
        "BELIEVE_TAB_02"
    ];

    internal static event Action<HistoryData, RunResult> CreateEntry;

    public static HistoryData TempEntry { get; set; } = new();

    internal static List<HistoryData> History { get; set; } = [];

    public static GlobalSaveData HistorySettings { get; set; } = new() { HistoryAmount = 50 };

    internal static GameObject ArchiveSprite { get; set; }

    public static ArchiveData Archive { get; set; } = new();

    #region Setup

    internal static void Initialize()
    {
        if (_active)
            return;
        LogManager.Log("Enable History controller");
        On.PlayMakerFSM.OnEnable += FsmEdits;
        IL.Breakable.Break += PreventTabletBreak;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        _active = true;
    }

    internal static void Unload()
    {
        TempEntry = null;
        if (!_active)
            return;
        LogManager.Log("Disable History controller");
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        IL.Breakable.Break -= PreventTabletBreak;
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        _active = false;
    }

    internal static void SetupList(LocalSaveData saveData)
    {
        History = saveData?.OldRunData;
        Archive = saveData?.Archive;
    }

    #endregion

    #region History Page

    private static IEnumerator ShowHistoryPage(PlayMakerFSM fsm)
    {
        GameObject board = new("History Board");
        try
        {
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            _pageIndex = 0;
            _elementLookUp.Clear();
            GameObject viewBlocker = new("View Blocker");
            viewBlocker.transform.SetParent(board.transform, true);
            viewBlocker.AddComponent<SpriteRenderer>().sprite = TreasureManager.BackgroundSprite;
            viewBlocker.GetComponent<SpriteRenderer>().sortingOrder = 1;
            viewBlocker.GetComponent<SpriteRenderer>().color = new(0f, 0f, 0f, 0.9f);
            viewBlocker.transform.localScale = new Vector3(54f, 26f, 1f);
            viewBlocker.transform.localPosition = new(0f, 0f);
            viewBlocker.SetActive(true);

            (SpriteRenderer, TextMeshPro) currentElement = TextManager.CreateUIObject("RunState");
            _elementLookUp.Add("RunState", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(0f, 6f);
            currentElement.Item2.alignment = TextAlignmentOptions.Center;
            currentElement.Item2.transform.localPosition = new(0f, 0f);
            board.layer = currentElement.Item1.gameObject.layer;
            viewBlocker.layer = board.layer;
            Component.Destroy(currentElement.Item1);

            float xPosition = -12f;
            float yPosition = 4f;
            for (int i = 1; i < 10; i++)
            {
                currentElement = TextManager.CreateUIObject("SeedSprite" + i);
                _elementLookUp.Add("SeedSprite" + i, currentElement);
                currentElement.Item1.transform.SetParent(board.transform);
                currentElement.Item1.transform.localPosition = new(xPosition, yPosition);
                currentElement.Item1.transform.localScale = new(1f, 1f);
                Component.Destroy(currentElement.Item2.gameObject);
                // The structure should match the alignment in the setting room.
                xPosition += 3f;
                if (xPosition > 0f)
                {
                    xPosition = -12f;
                    yPosition = 1f;
                }
                else if (xPosition == -6f && yPosition == 1f)
                    xPosition += 3f;
            }
            currentElement = TextManager.CreateUIObject("TakenTime");
            _elementLookUp.Add("TakenTime", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-14.5f, 0f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            List<string> values =
            [
                ScoreData.ScoreField,
                ScoreData.TimeField,
                ScoreData.KillStreakField,
                ScoreData.EssenceField,
                ScoreData.GrubField,
                ScoreData.TraverseField,
                ScoreData.PerfectBossesField,
                ScoreData.PerfectFinalBossField,
                ScoreData.FinalScoreField,
            ];

            for (int i = 0; i < values.Count; i++)
            {
                currentElement = TextManager.CreateUIObject(values[i]);
                _elementLookUp.Add(values[i], currentElement);
                currentElement.Item1.transform.SetParent(board.transform);
                currentElement.Item1.transform.localPosition = new(-14.5f, -0.75f - (i * 0.5f));
                currentElement.Item2.alignment = TextAlignmentOptions.Left;
                if (i == values.Count - 1)
                {
                    currentElement.Item1.transform.localPosition -= new Vector3(0f, 0.25f);
                    currentElement.Item2.fontSize = 4;
                }
                else
                    currentElement.Item2.fontSize = 2;
                Component.Destroy(currentElement.Item1);
            }

            currentElement = TextManager.CreateUIObject("CombatLevel");
            _elementLookUp.Add("CombatLevel", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, 0.25f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.color = Color.red;
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("SpiritLevel");
            _elementLookUp.Add("SpiritLevel", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -0.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.color = new(0.957f, 0.012f, 0.988f);
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("EnduranceLevel");
            _elementLookUp.Add("EnduranceLevel", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -1.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.color = Color.green;
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("FinalRoom");
            _elementLookUp.Add("FinalRoom", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -2.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("TotalCommonPowers");
            _elementLookUp.Add("TotalCommonPowers", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -3.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 3;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("TotalUncommonPowers");
            _elementLookUp.Add("TotalUncommonPowers", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -4.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 3;
            currentElement.Item2.color = new(0.2f, 0.8f, 0.2f);
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("TotalRarePowers");
            _elementLookUp.Add("TotalRarePowers", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-6.75f, -5.75f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 3;
            currentElement.Item2.color = Color.cyan;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("PowerList");
            _elementLookUp.Add("PowerList", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(6f, 5.5f);
            currentElement.Item2.alignment = TextAlignmentOptions.Center;
            currentElement.Item2.text = "Powers";
            currentElement.Item2.fontSize = 4;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("Version");
            _elementLookUp.Add("Version", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-15f, 6.7f);
            currentElement.Item2.alignment = TextAlignmentOptions.Left;
            currentElement.Item2.fontSize = 2;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("EntryNumber");
            _elementLookUp.Add("EntryNumber", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(11f, 6.9f);
            currentElement.Item2.alignment = TextAlignmentOptions.Right;
            currentElement.Item2.fontSize = 2;
            Component.Destroy(currentElement.Item1);

            currentElement = TextManager.CreateUIObject("ArrowLeft");
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-16f, 0f);
            currentElement.Item1.transform.localScale = new(2f, 2f);
            currentElement.Item1.transform.SetRotation2D(180f);
            currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
            GameObject.Destroy(currentElement.Item2.gameObject);

            currentElement = TextManager.CreateUIObject("ArrowRight");
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(16f, 0f);
            currentElement.Item1.transform.localScale = new(2f, 2f);
            currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
            GameObject.Destroy(currentElement.Item2.gameObject);

            currentElement = TextManager.CreateUIObject("UpperBorder");
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(0f, 7.5f);
            currentElement.Item1.transform.localScale = new(1.8f, 1.8f);
            currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.History_Upper");
            GameObject.Destroy(currentElement.Item2.gameObject);

            currentElement = TextManager.CreateUIObject("LowerBorder");
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(0f, -7.5f);
            currentElement.Item1.transform.localScale = new(1.8f, 1.8f);
            currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.History_Lower");
            GameObject.Destroy(currentElement.Item2.gameObject);

            currentElement = TextManager.CreateUIObject("Check");
            _elementLookUp.Add("Check", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-14f, 5.6f);
            currentElement.Item1.transform.localScale = new(0.2f, 0.2f);
            currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.ImprovedGrimmchild");
            GameObject.Destroy(currentElement.Item2.gameObject);

            currentElement = TextManager.CreateUIObject("Seeded");
            _elementLookUp.Add("Seeded", currentElement);
            currentElement.Item1.transform.SetParent(board.transform);
            currentElement.Item1.transform.localPosition = new(-8.9f, 6f);
            currentElement.Item2.text = "(Seeded)";
            currentElement.Item2.fontSize--;
            Component.Destroy(currentElement.Item1);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to display history page.", ex);
            GameObject.Destroy(board);
            yield break;
        }
        UpdatePage();
        float _scrollCooldown = 0.5f;
        while (true)
        {
            yield return null;
            if (_scrollCooldown <= 0f)
            {
                _scrollCooldown = 0.5f;
                if (InputHandler.Instance.inputActions.left.IsPressed)
                {
                    _pageIndex--;
                    if (_pageIndex == -1)
                        _pageIndex = History.Count - 1;
                    UpdatePage();
                }
                else if (InputHandler.Instance.inputActions.right.IsPressed)
                {
                    _pageIndex++;
                    if (_pageIndex == History.Count)
                        _pageIndex = 0;
                    UpdatePage();
                }
                else if (InputHandler.Instance.inputActions.up.IsPressed)
                    UpdatePowerList(false);
                else if (InputHandler.Instance.inputActions.down.IsPressed)
                    UpdatePowerList(true);
                else if (InputHandler.Instance.inputActions.jump.IsPressed)
                    break;
                else
                    _scrollCooldown = 0f;
            }
            else
                _scrollCooldown -= Time.deltaTime;

        }
        fsm.SendEvent("CONVO_FINISH");
        GameObject.Destroy(board);
    }

    private static void UpdatePage()
    {
        try
        {
            HistoryData currentHistory = History.Count != 0
                ? History[_pageIndex]
                : new()
                {
                    Score = new()
                    {
                        PassedTime = 10f
                    }
                };
            _elementLookUp["RunState"].Item2.text = currentHistory.Result.ToString();
            _elementLookUp["RunState"].Item2.color = currentHistory.Result switch
            {
                RunResult.Completed => new(0.2f, 0.8f, 0.2f),
                RunResult.Failed => Color.red,
                _ => new(1f, 1f, 0f)
            };
            for (int i = 1; i < 10; i++)
            {
                // You might be tempted to think "Would it not be easier if the string was just saved as a string?". Yeah... but I'm stubborn and this was the first version.
                int spriteNumber = int.Parse("" + currentHistory.Seed.ToString()[i - 1]);
                _elementLookUp["SeedSprite" + i].Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities." + _seedSprites[spriteNumber]);
            }
            _elementLookUp["TakenTime"].Item2.text = "Final time: " + (currentHistory.Score.PassedTime >= 3600
                ? new DateTime(TimeSpan.FromSeconds(currentHistory.Score.PassedTime).Ticks).ToString("HH:mm:ss.ff")
                : new DateTime(TimeSpan.FromSeconds(currentHistory.Score.PassedTime).Ticks).ToString("mm:ss.ff"));

            Dictionary<string, int> scoreValues = currentHistory.Score.TransformToDictionary();

            foreach (var item in scoreValues.Keys)
                if (item != ScoreData.TimeField && item != ScoreData.FinalScoreField || currentHistory.Result == RunResult.Completed)
                    _elementLookUp[item].Item2.text = $"{item} {scoreValues[item]}";
                else
                    _elementLookUp[item].Item2.text = $"{item} -";

            _elementLookUp["CombatLevel"].Item2.text = $"Combat Level: {currentHistory.FinalCombatLevel}";
            _elementLookUp["SpiritLevel"].Item2.text = $"Spirit Level: {currentHistory.FinalSpiritLevel}";
            _elementLookUp["EnduranceLevel"].Item2.text = $"Endurance Level: {currentHistory.FinalEnduranceLevel}";
            _elementLookUp["FinalRoom"].Item2.text = $"Final room: {currentHistory.FinalRoomNumber}";
            _elementLookUp["TotalCommonPowers"].Item2.text = $"Common powers: {currentHistory.CommonPowerAmount}";
            _elementLookUp["TotalUncommonPowers"].Item2.text = $"Uncommon powers: {currentHistory.UncommonPowerAmount}";
            _elementLookUp["TotalRarePowers"].Item2.text = $"Rare powers: {currentHistory.RarePowerAmount}";
            string gameMode = currentHistory.Score.Mode switch
            {
                GameMode.Crusader => "Crusader",
                GameMode.GrandCrusader => "Grand Crusader",
                _ => "Unknown"
            };
            _elementLookUp["Version"].Item2.text = $"Version: {currentHistory.ModVersion} (Game Mode: {gameMode})";
            _elementLookUp["EntryNumber"].Item2.text = $"Entry number: ({_pageIndex + 1}/{History.Count})";

            _elementLookUp["Check"].Item1.gameObject.SetActive(!currentHistory.CheckRun());
            _elementLookUp["Seeded"].Item2.gameObject.SetActive(currentHistory.Seeded);
            UpdatePowerList(true, true);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to update history page.", ex);
        }
    }

    private static void UpdatePowerList(bool down, bool setup = false)
    {
        try
        {
            HistoryData currentHistory = History[_pageIndex];
            int maxPages = Mathf.CeilToInt(currentHistory.Powers.Count / 15f);
            if (setup)
                _powerPageIndex = 0;
            else if (down)
            {
                _powerPageIndex++;
                if (_powerPageIndex == maxPages)
                    _powerPageIndex = 0;
            }
            else
            {
                _powerPageIndex--;
                if (_powerPageIndex == -1)
                    _powerPageIndex = maxPages - 1;
            }
            foreach (Transform item in _elementLookUp["PowerList"].Item2.transform)
                GameObject.Destroy(item.gameObject);
            float xPosition = -3.3f;
            float yPosition = -1.2f;
            // Only display 15 per page.
            List<string> pagePowers = [.. currentHistory.Powers.Skip(_powerPageIndex * 15).Take(15)];
            for (int i = 0; i < 15; i++)
            {
                (SpriteRenderer, TextMeshPro) currentElement = TextManager.CreateUIObject(i >= pagePowers.Count ? "Dummy" : pagePowers[i]);
                currentElement.Item1.transform.SetParent(_elementLookUp["PowerList"].Item2.transform);
                currentElement.Item1.transform.localPosition = new(xPosition, yPosition);
                currentElement.Item2.alignment = TextAlignmentOptions.Center;
                currentElement.Item2.fontSize = 2;
                currentElement.Item2.enableWordWrapping = true;
                currentElement.Item2.textContainer.size = new(2f, 1f);
                Power power = i >= pagePowers.Count
                    ? null
                    : TreasureManager.Powers.FirstOrDefault(x => x.Name == pagePowers[i]);
                if (power != null)
                    currentElement.Item2.color = power.Tier switch
                    {
                        Rarity.Rare => Color.cyan,
                        Rarity.Uncommon => new(0.2f, 0.8f, 0.2f),
                        _ => Color.white
                    };
                currentElement.Item2.text = power?.Name ?? "-";
                Component.Destroy(currentElement.Item1);
                xPosition += 2.3f;
                if (xPosition > 1.3f)
                {
                    xPosition = -3.3f;
                    yPosition -= 1f;
                }
            }

            if (maxPages > 1)
            {
                _elementLookUp["PowerList"].Item2.text = $"Powers ({_powerPageIndex + 1}/ {maxPages})";
                (SpriteRenderer, TextMeshPro) currentElement = TextManager.CreateUIObject("ArrowUp");
                currentElement.Item1.transform.SetParent(_elementLookUp["PowerList"].Item2.transform);
                currentElement.Item1.transform.localPosition = new(0f, -1f);
                currentElement.Item1.transform.SetRotation2D(90f);
                currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
                GameObject.Destroy(currentElement.Item2.gameObject);

                currentElement = TextManager.CreateUIObject("ArrowDown");
                currentElement.Item1.transform.SetParent(_elementLookUp["PowerList"].Item2.transform);
                currentElement.Item1.transform.localPosition = new(0f, -6.2f);
                currentElement.Item1.transform.SetRotation2D(270f);
                currentElement.Item1.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
                GameObject.Destroy(currentElement.Item2.gameObject);
            }
            else
                _elementLookUp["PowerList"].Item2.text = "Powers";
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to update power list.", ex);
        }
    }

    #endregion

    #region Archive


    #endregion

    private static void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (self.FsmName == "inspect_region" && self.transform.parent != null && self.transform.parent.parent != null && self.transform.parent.parent.name.Contains("Plaque_statue_"))
            {
                if (self.transform.parent.parent.name == "Plaque_statue_02 (3)")
                {
                    History ??= [];
                    if (History.Count != 0)
                    {
                        self.AddState("Show History", () => TrialOfCrusaders.Holder.StartCoroutine(ShowHistoryPage(self)), FsmTransitionData.FromTargetState("Look Up End?").WithEventName("CONVO_FINISH"));
                        self.GetState("Centre?").AdjustTransitions("Show History");
                    }
                    GameObject archiveTablet = GameObject.Instantiate(ArchiveSprite);
                    archiveTablet.name = "Archive tablet";
                    archiveTablet.SetActive(true);
                    archiveTablet.transform.position = new(47.8f, 33.7f, 0.01f);
                    // This causes the tablet to black out (idk why)
                    archiveTablet.transform.localScale = new(1.2f, 1.1f);


                    GameObject archiveInspect = GameObject.Instantiate(HubController.InspectPrefab);
                    archiveInspect.name = "Archive Inspect";
                    archiveInspect.SetActive(true);
                    archiveInspect.transform.position = new(47.8f, 31.3f);
                    archiveInspect.LocateMyFSM("inspect_region").FsmVariables.FindFsmString("Game Text Convo").Value = "ToC_TBA";

                    GameObject blocker = new("Blocker");
                    blocker.transform.position = new(27.12f, 12.5f);
                    blocker.layer = 8;
                    blocker.AddComponent<BoxCollider2D>().size = new(10f, 1f);

                    blocker = new("Blocker 2");
                    blocker.transform.position = new(55.01f, 12.5f);
                    blocker.layer = 8;
                    blocker.AddComponent<BoxCollider2D>().size = new(11f, 1f);

                    blocker = new("Blocker 3");
                    blocker.transform.position = new(84f, 12.5f);
                    blocker.layer = 8;
                    blocker.AddComponent<BoxCollider2D>().size = new(11f, 1f);

                    blocker = new("Blocker 3");
                    blocker.transform.position = new(108.45f, 17.775f);
                    blocker.layer = 8;
                    blocker.AddComponent<BoxCollider2D>().size = new(1f, 30f);
                }
                else
                {
                    orig(self);
                    if (self.transform.parent.parent.name.StartsWith("Plaque_statue_03") 
                        || !_tabletKeys.Contains(self.FsmVariables.FindFsmString("Game Text Convo").Value))
                        GameObject.Destroy(self.transform.parent.parent.gameObject);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify history fsm", ex);
        }
        orig(self);
    }

    private static void PreventTabletBreak(MonoMod.Cil.ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<Breakable>("isBroken"));
        cursor.EmitDelegate<Func<bool, bool>>(x => x || GameManager.instance.sceneName == "Dream_Room_Believer_Shrine");
    }

    internal static void AddEntry(RunResult result)
    {
        TempEntry ??= new();
        CreateEntry?.Invoke(TempEntry, result);
        // Take RngProvider data
        TempEntry.Seed = RngManager.Seed;
        TempEntry.Seeded = RngManager.Seeded;

        // Set the non-controller related data.
        TempEntry.ModVersion = TrialOfCrusaders.Instance.GetVersion();
        TempEntry.Result = result;
        // In the case of completed runs the result is written seperately (as the crowd still throws missable geo).
        if (result != RunResult.Completed)
        {
            if ((result == RunResult.Failed && HistorySettings.TrackFailedRuns)
                || (result == RunResult.Forfeited && HistorySettings.TrackForfeitedRuns))
            {
                TempEntry.RunId = TempEntry.GetRunId();
                History.Add(TempEntry);
                if (History.Count > HistorySettings.HistoryAmount)
                    History.RemoveAt(0);
            }
            TempEntry = null;
        }
        else
            LogManager.Log("Added entry to history");
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "BELIEVE_TAB_04")
            return LobbyDialog.HistoryEmpty;
        else if (key == "ToC_TBA")
            return "Whatever this might be, at the moment it has not been fully constructed yet. You might wanna come back later.";
        else if (_tabletKeys.Contains(key))
        {
            bool unlocked = false;
            switch (key)
            {
                case "BELIEVE_TAB_07":
                    unlocked = Archive.FastestTrial != 0;
                    break;
                case "BELIEVE_TAB_24":
                    unlocked = Archive.FastestGrandTrial != 0;
                    break;
                case "BELIEVE_TAB_08":
                    unlocked = Archive.HighestTrialScore >= 5000 | Archive.HighestGrandTrialScore >= 5000;
                    break;
                case "BELIEVE_TAB_36":
                    unlocked = Archive.HighestTrialScore >= 10000 | Archive.HighestGrandTrialScore >= 10000;
                    break;
                case "BELIEVE_TAB_05":
                    unlocked = (Archive.FastestGrandTrial < 3600 && Archive.FastestGrandTrial > 0) | (Archive.FastestTrial < 3600 && Archive.FastestTrial > 0);
                    break;
                case "BELIEVE_TAB_01":
                    unlocked = Archive.CommonOnlyRun;
                    break;
                case "BELIEVE_TAB_35":
                    unlocked = Archive.GrubRecord >= 10;
                    break;
                case "BELIEVE_TAB_57":
                    unlocked = Archive.EssenceRecord >= 50;
                    break;
                case "BELIEVE_TAB_03":
                    unlocked = Archive.HighestTrialScore >= 15000 | Archive.HighestGrandTrialScore >= 15000;
                    break;
                case "BELIEVE_TAB_06":
                    unlocked = Archive.FinishedSeededRun;
                    break;
                case "BELIEVE_TAB_09":
                    unlocked = Archive.HighestTrialScore >= 20000 | Archive.HighestGrandTrialScore >= 20000;
                    break;
                case "BELIEVE_TAB_02":
                    unlocked = Archive.PerfectFinalBoss;
                    break;
            }

            if (unlocked)
            {
                string archiveText = ArchiveText.ResourceManager.GetString(key);
                if (key == "BELIEVE_TAB_07")
                    archiveText = string.Format(archiveText, TreasureManager.Powers.Length);
                if (SecretController.UnlockedSecretArchive)
                {
                    string secretText = ArchiveText.ResourceManager.GetString($"Secret_{key}");
                    archiveText += $"<page>{secretText}";
                    if (key == "BELIEVE_TAB_07")
                        archiveText = string.Format(archiveText, TreasureManager.Powers.Count(x => x.Tier == Rarity.Common), 
                            TreasureManager.Powers.Count(x => x.Tier == Rarity.Uncommon), TreasureManager.Powers.Count(x => x.Tier == Rarity.Rare));
                }
                return archiveText;
            }
            else
                return string.Format(ArchiveText.LockedPrompt, ArchiveText.ResourceManager.GetString($"Locked_{key}"));
        }
        return orig;
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == nameof(PlayerData.hasDash) || name == nameof(PlayerData.canDash) || name == nameof(PlayerData.hasWalljump))
            return true;
        return orig;
    }

    internal static void CheckArchiveUpdate()
    {
        Archive ??= new();
        Archive.EssenceRecord = Math.Max(Archive.EssenceRecord, PDHelper.DreamOrbs);
        Archive.GrubRecord = Math.Max(Archive.EssenceRecord, ScoreController.Score.GrubBonus);
        // 2 uncommon powers (dive and fireball) are unavoidable, they will be excluded.
        Archive.CommonOnlyRun = Archive.CommonOnlyRun
            | (TempEntry.UncommonPowerAmount <= 2 && TempEntry.RarePowerAmount == 0);
        Archive.FinishedSeededRun = Archive.FinishedSeededRun | RngManager.Seeded;
        Archive.PerfectFinalBoss = ScoreController.Score.HitlessFinalBoss | Archive.PerfectFinalBoss;
        Dictionary<string, int> scoreDictionary = ScoreController.Score.TransformToDictionary();
        if (ScoreController.Score.Mode == GameMode.GrandCrusader)
        {
            Archive.FastestGrandTrial = Math.Min(Archive.FastestGrandTrial, ScoreController.Score.PassedTime);
            Archive.HighestGrandTrialScore = Math.Max(Archive.HighestGrandTrialScore, scoreDictionary[ScoreData.FinalScoreField]);
        }
        else
        {
            Archive.FastestTrial = Math.Min(Archive.FastestTrial, ScoreController.Score.PassedTime);
            Archive.HighestTrialScore = Math.Max(Archive.HighestTrialScore, scoreDictionary[ScoreData.FinalScoreField]);
        }
    }
}
