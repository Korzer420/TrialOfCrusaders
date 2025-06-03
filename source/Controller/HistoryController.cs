using KorzUtils.Data;
using KorzUtils.Helper;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
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

    internal static event Action<HistoryData, RunResult> CreateEntry;

    public static HistoryData TempEntry { get; set; } = new();

    internal static List<HistoryData> History { get; set; } = [];

    #region Setup

    internal static void Initialize()
    {
        if (_active)
            return;
        LogManager.Log("Enable history controller");
        On.PlayMakerFSM.OnEnable += FsmEdits;
        IL.Breakable.Break += PreventTabletBreak;
        _active = true;
    }

    internal static void Unload()
    {
        TempEntry = null;
        if (!_active)
            return;
        LogManager.Log("Disable history controller");
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        IL.Breakable.Break -= PreventTabletBreak;
        _active = false;
    }

    internal static void SetupList(List<HistoryData> history) => History = history;

    #endregion

    #region History Page

    private static IEnumerator ShowPage(PlayMakerFSM fsm)
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
            currentElement.Item2.fontSize = 4;
            Component.Destroy(currentElement.Item1);

            List<string> values =
            [
                "Score",
            "Essence bonus",
            "Time bonus",
            "Killstreak bonus",
            "Flawless stage bonus",
            "Perfect streak bonus",
            "Perfect boss bonus",
            "Perfect final bonus",
            "Final score"
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
            currentElement.Item2.color = new(0.5f, 0f, 0.5f);
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
        GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        GameObject.Destroy(board);
    }

    private static void UpdatePage()
    {
        try
        {
            HistoryData currentHistory = History[_pageIndex];
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

            List<(string, int)> values =
            [
                new("Score", currentHistory.Score.Score),
            new("Essence bonus",  currentHistory.Score.Essence * 10),
            new("Time bonus", currentHistory.Result == RunResult.Completed ? Math.Max(0, 3600 - Mathf.CeilToInt(currentHistory.Score.PassedTime)) : -1),
            new("Killstreak bonus", currentHistory.Score.HighestKillStreak * 5),
            new("Flawless stage bonus", currentHistory.Score.TotalHitlessRooms * 20),
            new("Perfect streak bonus", currentHistory.Score.HighestHitlessRoomStreak * 20),
            new("Perfect boss bonus", currentHistory.Score.HitlessBosses * 200),
            new("Perfect final bonus", currentHistory.Score.HitlessFinalBoss ? 1000 : 0),
        ];
            values.Add(new("Final score", values.Select(x => x.Item2).Sum()));

            foreach (var item in values)
                if (item.Item1 != "Time bonus" && item.Item1 != "Final score" || currentHistory.Result == RunResult.Completed)
                    _elementLookUp[item.Item1].Item2.text = $"{item.Item1}: {item.Item2}";
                else
                    _elementLookUp[item.Item1].Item2.text = $"{item.Item1}: -";

            _elementLookUp["CombatLevel"].Item2.text = $"Combat Level: {currentHistory.FinalCombatLevel}";
            _elementLookUp["SpiritLevel"].Item2.text = $"Spirit Level: {currentHistory.FinalSpiritLevel}";
            _elementLookUp["EnduranceLevel"].Item2.text = $"Endurance Level: {currentHistory.FinalEnduranceLevel}";
            _elementLookUp["FinalRoom"].Item2.text = $"Final room: {currentHistory.FinalRoomNumber}";
            _elementLookUp["TotalCommonPowers"].Item2.text = $"Common powers: {currentHistory.CommonPowerAmount}";
            _elementLookUp["TotalUncommonPowers"].Item2.text = $"Uncommon powers: {currentHistory.UncommonPowerAmount}";
            _elementLookUp["TotalRarePowers"].Item2.text = $"Rare powers: {currentHistory.RarePowerAmount}";
            _elementLookUp["Version"].Item2.text = $"Version: {currentHistory.ModVersion}";
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

    private static void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (self.FsmName == "inspect_region" && self.transform.parent != null && self.transform.parent.parent != null && self.transform.parent.parent.name.Contains("Plaque_statue_"))
            {
                if (self.transform.parent.parent.name == "Plaque_statue_02 (3)")
                {
                    self.AddState("Show History", () => TrialOfCrusaders.Holder.StartCoroutine(ShowPage(self)), FsmTransitionData.FromTargetState("Look Up End?").WithEventName("CONVO_FINISH"));
                    self.GetState("Centre?").AdjustTransitions("Show History");
                    GameObject blocker = new("Blocker");
                    blocker.transform.position = new(22.85f, 17.39f);
                    blocker.layer = 8;
                    blocker.AddComponent<BoxCollider2D>().size = new(1f, 10f);
                }
                else
                {
                    orig(self);
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
        TempEntry.GameMode = GameMode.Normal;
        // In the case of completed runs the result is written seperately (as the crowd still throws missable geo).
        if (result != RunResult.Completed)
        {
            TempEntry.RunId = TempEntry.GetRunId();
            History.Add(TempEntry);
            TempEntry = null;
        }
        else
            LogManager.Log("Added entry to history");
    }
}
