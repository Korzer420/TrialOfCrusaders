using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

internal static class InventoryController
{
    private const string CombatStat = "Combat Stat";
    private const string SpiritStat = "Spirit Stat";
    private const string EnduranceStat = "Endurance Stat";
    private const string FirstPowerSlot = "First Power Slot";
    private const string SecondPowerSlot = "Second Power Slot";
    private const string ThirdPowerSlot = "Third Power Slot";
    private const string FourthPowerSlot = "Fourth Power Slot";
    private const string MainSprite = "MainSprite";
    private const string PowerName = "Power Name";
    private const string PowerPool = "Power Pool";
    private const string PowerRarity = "Power Rarity";
    private const string PowerScaling = "Power Scaling";
    private const string PowerDescription = "Power Description";
    private const string PowerListDown = "Power List Down";
    private const string PowerListUp = "Power List Up";
    public const string Selector = "Selector";

    #region Draft Pool Text Color

    private const string WealthTextColor = "#ffdd00"; // Yellow
    private const string InstantTextColor = "#ffffff"; // White
    private const string RiskTextColor = "#ffa600"; // Orange
    private const string TreasureTextColor = "#faefa7";
    private const string AbilityTextColor = "#00ebe3"; // Cyan
    private const string CharmTextColor = "#ff85fb"; // Pink
    private const string UpgradeTextColor = "#b39b34"; // Beige
    private const string BurstTextColor = "#00eba8";
    private const string DebuffTextColor = "#694fff"; // Blue

    #endregion

    internal static Dictionary<string, (SpriteRenderer, TextMeshPro)> ElementLookup = [];
    private static PlayMakerFSM _inventoryFsm;
    private static bool _enabled;

    #region Inventory setup

    internal static void CreateInventoryPage(GameObject inventoryPage)
    {
        if (inventoryPage.transform.childCount > 1)
            return;
        ElementLookup.Clear();
        CreateStatElements(inventoryPage.transform);
        CreatePowerList(inventoryPage.transform);
        CreateSeperators(inventoryPage.transform);
        CreateMainSprite(inventoryPage.transform);
        CreatePowerDetails(inventoryPage.transform);

        _inventoryFsm = inventoryPage.LocateMyFSM("Empty UI");
        SetupFsm(_inventoryFsm);
    }

    private static void CreateStatElements(Transform parent)
    {
        GameObject statBox = new("Stat box");
        statBox.layer = 5;
        statBox.transform.SetParent(parent);
        statBox.transform.localPosition = new(-4f, -12.5f);

        TextMeshPro currentElement = CreateTextElement(true);
        currentElement.name = CombatStat;
        currentElement.transform.SetParent(statBox.transform);
        currentElement.transform.localPosition = new(0f, 1f);
        currentElement.alignment = TextAlignmentOptions.Left;
        currentElement.text = $"<color={CombatController.CombatStatColor}>Combat Level: {CombatController.CombatLevel}</color>";
        currentElement.fontSize = 3;
        ElementLookup.Add(CombatStat, new(null, currentElement));

        currentElement = CreateTextElement(true);
        currentElement.name = SpiritStat;
        currentElement.transform.SetParent(statBox.transform);
        currentElement.transform.localPosition = new(0f, 0f);
        currentElement.alignment = TextAlignmentOptions.Left;
        currentElement.text = $"<color={CombatController.SpiritStatColor}>Spirit Level: {CombatController.SpiritLevel}</color>";
        currentElement.fontSize = 3;
        ElementLookup.Add(SpiritStat, new(null, currentElement));

        currentElement = CreateTextElement(true);
        currentElement.name = EnduranceStat;
        currentElement.transform.SetParent(statBox.transform);
        currentElement.transform.localPosition = new(0f, -1f);
        currentElement.alignment = TextAlignmentOptions.Left;
        currentElement.text = $"<color={CombatController.EnduranceStatColor}>Endurance Level: {CombatController.EnduranceLevel}</color>";
        currentElement.fontSize = 3;
        ElementLookup.Add(EnduranceStat, new(null, currentElement));
    }

    private static void CreatePowerList(Transform parent)
    {
        GameObject powerList = new("Power List");
        powerList.transform.SetParent(parent);
        powerList.layer = 5;
        powerList.transform.localPosition = new(-4.4f, -6f);
        powerList.AddComponent<BoxCollider2D>().size = new(7f, 6f);

        TextMeshPro currentElement = CreateTextElement(true);
        currentElement.name = FirstPowerSlot;
        currentElement.transform.SetParent(powerList.transform);
        currentElement.transform.localPosition = new(0f, 2.25f, -0.1f);
        currentElement.alignment = TextAlignmentOptions.Center;
        currentElement.text = $"-";
        currentElement.fontSize = 3;
        ElementLookup.Add(FirstPowerSlot, new(null, currentElement));

        currentElement = CreateTextElement(true);
        currentElement.name = SecondPowerSlot;
        currentElement.transform.SetParent(powerList.transform);
        currentElement.transform.localPosition = new(0f, 0.75f, -0.1f);
        currentElement.alignment = TextAlignmentOptions.Center;
        currentElement.text = $"-";
        currentElement.fontSize = 3;
        ElementLookup.Add(SecondPowerSlot, new(null, currentElement));

        currentElement = CreateTextElement(true);
        currentElement.name = ThirdPowerSlot;
        currentElement.transform.SetParent(powerList.transform);
        currentElement.transform.localPosition = new(0f, -0.75f, -0.1f);
        currentElement.alignment = TextAlignmentOptions.Center;
        currentElement.text = $"-";
        currentElement.fontSize = 3;
        ElementLookup.Add(ThirdPowerSlot, new(null, currentElement));

        currentElement = CreateTextElement(true);
        currentElement.name = FourthPowerSlot;
        currentElement.transform.SetParent(powerList.transform);
        currentElement.transform.localPosition = new(0f, -2.25f, -0.1f);
        currentElement.alignment = TextAlignmentOptions.Center;
        currentElement.text = $"-";
        currentElement.fontSize = 3;
        ElementLookup.Add(FourthPowerSlot, new(null, currentElement));

        GameObject selectorPrefab = parent.parent.Find("Journal/selector").gameObject;
        GameObject selector = GameObject.Instantiate(selectorPrefab, powerList.transform);
        selector.name = Selector;
        selector.transform.localPosition = new(0f, 2.25f);
        selector.transform.localScale = new Vector3(2.7f, 1.6f, -0.1f);
        ElementLookup.Add(Selector, new(selector.GetComponent<SpriteRenderer>(), null));

        GameObject arrowObject = new(PowerListUp);
        arrowObject.layer = 5;
        arrowObject.transform.SetParent(powerList.transform);
        arrowObject.transform.localPosition = new(0f, 4f);
        arrowObject.transform.localScale = new(2f, 2f);
        arrowObject.transform.SetRotation2D(90);
        SpriteRenderer spriteRenderer = arrowObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
        spriteRenderer.sortingLayerID = 629535577;
        spriteRenderer.sortingLayerName = "HUD";
        ElementLookup.Add(PowerListUp, new(spriteRenderer, null));

        arrowObject = new(PowerListDown);
        arrowObject.layer = 5;
        arrowObject.transform.SetParent(powerList.transform);
        arrowObject.transform.localPosition = new(0f, -4f);
        arrowObject.transform.localScale = new(2f, 2f);
        arrowObject.transform.SetRotation2D(270);
        spriteRenderer = arrowObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
        spriteRenderer.sortingLayerID = 629535577;
        spriteRenderer.sortingLayerName = "HUD";
        ElementLookup.Add(PowerListDown, new(spriteRenderer, null));
    }

    private static void CreateSeperators(Transform parent)
    {
        GameObject seperatorHolder = new("Seperator Holder");
        seperatorHolder.transform.SetParent(parent);

        GameObject dividerPrefab = parent.parent.Find("Inv/Divider L").gameObject;
        GameObject divider = GameObject.Instantiate(dividerPrefab, seperatorHolder.transform);
        divider.name = "Left divider";
        divider.transform.position = new(-4f, 0f);

        divider = GameObject.Instantiate(dividerPrefab, seperatorHolder.transform);
        divider.name = "Right divider";
        divider.transform.position = new(4f, 0f);
    }

    private static void CreateMainSprite(Transform parent)
    {
        GameObject gameObject = new(MainSprite);
        gameObject.layer = 5;
        gameObject.transform.SetParent(parent);
        gameObject.transform.localPosition = new(4.2f, -7.2f);
        gameObject.transform.localScale = new(2f, 2f);
        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
        spriteRenderer.sortingLayerID = 629535577;
        spriteRenderer.sortingLayerName = "HUD";
        ElementLookup.Add(MainSprite, new(spriteRenderer, null));
    }

    private static void CreatePowerDetails(Transform parent)
    {
        GameObject powerDetails = new("Power details");
        powerDetails.layer = 5;
        powerDetails.transform.SetParent(parent);
        powerDetails.transform.localPosition = new(13f, -7f);

        TextMeshPro currentElement = CreateTextElement(true);
        currentElement.name = PowerName;
        currentElement.transform.SetParent(powerDetails.transform);
        currentElement.transform.localPosition = new(0f, 0f);
        currentElement.text = "";
        ElementLookup.Add(PowerName, new(null, currentElement));

        GameObject seperatorPrefab = parent.parent.Find("Charms/Equipped Charms/Inv_0017_divider").gameObject;
        GameObject seperator = GameObject.Instantiate(seperatorPrefab, powerDetails.transform);
        seperator.transform.localPosition = new(0, 4f);
        seperator.transform.localScale = new(5f, 1f);

        currentElement = CreateTextElement();
        currentElement.name = PowerPool;
        currentElement.transform.SetParent(powerDetails.transform);
        currentElement.transform.localPosition = new(0f, -1.7f);
        currentElement.text = $"";
        ElementLookup.Add(PowerPool, new(null, currentElement));

        currentElement = CreateTextElement();
        currentElement.name = PowerScaling;
        currentElement.transform.SetParent(powerDetails.transform);
        currentElement.transform.localPosition = new(0f, -4f);
        currentElement.text = $"";
        ElementLookup.Add(PowerScaling, new(null, currentElement));

        currentElement = CreateTextElement();
        currentElement.name = PowerRarity;
        currentElement.transform.SetParent(powerDetails.transform);
        currentElement.transform.localPosition = new(0f, -4.7f);
        currentElement.text = $"";
        ElementLookup.Add(PowerRarity, new(null, currentElement));

        seperator = GameObject.Instantiate(seperatorPrefab, powerDetails.transform);
        seperator.transform.localPosition = new(0, -0.3f);
        seperator.transform.localScale = new(5f, 1f);

        currentElement = CreateTextElement();
        currentElement.name = PowerDescription;
        currentElement.transform.SetParent(powerDetails.transform);
        currentElement.transform.localPosition = new(0f, -6f);
        currentElement.text = "";
        ElementLookup.Add(PowerDescription, new(null, currentElement));
    }

    internal static TextMeshPro CreateTextElement(bool isTitle = false)
    {
        GameObject textElement = isTitle
            ? GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject)
            : GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        return textElement.GetComponent<TextMeshPro>();
    }

    private static void SetupFsm(PlayMakerFSM fsm)
    {
        fsm.AddVariable("PowerIndex", 0);
        fsm.AddVariable("PowerIndexFirstSlot", 0);

        // Removing the jump from arrow button to arrow button.
        fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
        fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

        FsmState currentWorkingState = fsm.GetState("Init Heart Piece");
        currentWorkingState.Name = "Init Powers";

        GameObject cursor = fsm.transform.Find("Cursor").gameObject;
        fsm.AddState("Update selection", () =>
        {
            int index = fsm.FsmVariables.FindFsmInt("PowerIndex").Value;
            int powerIndex = fsm.FsmVariables.FindFsmInt("PowerIndexFirstSlot").Value;
            ElementLookup[PowerListUp].Item1.gameObject.SetActive(powerIndex != 0);
            ElementLookup[PowerListDown].Item1.gameObject.SetActive(powerIndex + 3 < CombatController.ObtainedPowers.Count);
            index = powerIndex + index;
            if (index >= CombatController.ObtainedPowers.Count)
                LogManager.Log("Indexed power slot out of range.", KorzUtils.Enums.LogType.Error);
            else
            {
                ElementLookup[Selector].Item1.transform.localPosition = new(0f, 2.25f - (index - powerIndex) * 1.5f);
                Power selectedPower = CombatController.ObtainedPowers[index];
                ElementLookup[PowerName].Item2.text = selectedPower.Name;
                ElementLookup[PowerDescription].Item2.text = selectedPower.Description;
                ElementLookup[PowerRarity].Item2.text = selectedPower.Tier switch
                {
                    Enums.Rarity.Rare => $"Rarity: <color={TreasureManager.RareTextColor}>Rare</color>",
                    Enums.Rarity.Uncommon => $"Rarity: <color={TreasureManager.UncommonTextColor}>Uncommon</color>",
                    _ => $"Rarity: Common"
                };

                string scalingText = string.Empty;
                if (selectedPower.Scaling.HasFlag(Enums.StatScaling.Combat))
                    scalingText += $"<color={CombatController.CombatStatColor}>Combat</color>,";
                if (selectedPower.Scaling.HasFlag(Enums.StatScaling.Spirit))
                    scalingText += $" <color={CombatController.SpiritStatColor}>Spirit</color>,";
                if (selectedPower.Scaling.HasFlag(Enums.StatScaling.Endurance))
                    scalingText += $" <color={CombatController.EnduranceStatColor}>Endurance</color>,";
                if (string.IsNullOrEmpty(scalingText))
                    scalingText = "-";
                else
                    scalingText = scalingText.TrimEnd(',').TrimStart('\0');
                ElementLookup[PowerScaling].Item2.text = $"Scaling: {scalingText}";
                ElementLookup[MainSprite].Item1.sprite = selectedPower.Sprite;

                string pool = string.Empty;
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Combat))
                    pool += $"<color={CombatController.CombatStatColor}>Combat</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Spirit))
                    pool += $" <color={CombatController.SpiritStatColor}>Spirit</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Endurance))
                    pool += $" <color={CombatController.EnduranceStatColor}>Endurance</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Wealth))
                    pool += $" <color={WealthTextColor}>Wealth</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Instant))
                    pool += $" <color={InstantTextColor}>Instant</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Risk))
                    pool += $" <color={RiskTextColor}>Risk</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Treasure))
                    pool += $" <color={TreasureTextColor}>Treasure</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Ability))
                    pool += $" <color={AbilityTextColor}>Ability</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Charm))
                    pool += $" <color={CharmTextColor}>Charm</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Upgrade))
                    pool += $" <color={UpgradeTextColor}>Upgrade</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Burst))
                    pool += $" <color={BurstTextColor}>Burst</color>,";
                if (selectedPower.Pools.HasFlag(Enums.DraftPool.Debuff))
                    pool += $" <color={DebuffTextColor}>Debuff</color>,";
                pool = pool.Trim().Trim(',');
                ElementLookup[PowerPool].Item2.text = $"Pools: {pool}";
            }
        }, FsmTransitionData.FromTargetState("L Arrow").WithEventName("UI LEFT"),
            FsmTransitionData.FromTargetState("R Arrow").WithEventName("UI RIGHT"));

        fsm.AddState("Move to selection", [new SetSpriteRendererOrder()
        {
            gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow") },
            order = 0,
            delay = 0f
        }, new GenericFsmStateAction(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR"))],
        FsmTransitionData.FromTargetState("Update selection").WithEventName("FINISHED"));

        fsm.AddState("Check from left", () =>
        {
            if (CombatController.ObtainedPowers.Count == 0)
                fsm.SendEvent("UI RIGHT");
            else
                fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value =
                    ElementLookup[FirstPowerSlot].Item2.transform.parent.gameObject;
        }, FsmTransitionData.FromTargetState("Move to selection").WithEventName("FINISHED"),
            FsmTransitionData.FromTargetState("R Arrow").WithEventName("UI RIGHT"));

        fsm.AddState("Check from right", () =>
        {
            if (CombatController.ObtainedPowers.Count == 0)
                fsm.SendEvent("UI LEFT");
            else
                fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value =
                    ElementLookup[FirstPowerSlot].Item2.transform.parent.gameObject;
        }, FsmTransitionData.FromTargetState("Move to selection").WithEventName("FINISHED"),
            FsmTransitionData.FromTargetState("L Arrow").WithEventName("UI LEFT"));

        fsm.AddState("Move up", () =>
        {
            int currentIndex = fsm.FsmVariables.FindFsmInt("PowerIndex").Value;
            int currentFirstIndex = fsm.FsmVariables.FindFsmInt("PowerIndexFirstSlot").Value;
            if (currentIndex > 0)
                fsm.FsmVariables.FindFsmInt("PowerIndex").Value--;
            else
            {
                if (currentFirstIndex == 0)
                    return;
                currentFirstIndex--;
                UpdateList(currentFirstIndex);
            }
        }, FsmTransitionData.FromTargetState("Update selection").WithEventName("FINISHED"));

        fsm.AddState("Move down", () =>
        {
            int currentIndex = fsm.FsmVariables.FindFsmInt("PowerIndex").Value;
            int currentFirstIndex = fsm.FsmVariables.FindFsmInt("PowerIndexFirstSlot").Value;
            if (currentIndex < 3)
            {
                if (currentIndex + 1 >= CombatController.ObtainedPowers.Count)
                    return;
                fsm.FsmVariables.FindFsmInt("PowerIndex").Value++;
            }
            else
            {
                if (currentFirstIndex >= CombatController.ObtainedPowers.Count - 4)
                    return;
                currentFirstIndex++;
                UpdateList(currentFirstIndex);
            }
        }, FsmTransitionData.FromTargetState("Update selection").WithEventName("FINISHED"));

        fsm.GetState("Update selection").AddTransition("UI UP", "Move up");
        fsm.GetState("Update selection").AddTransition("UI DOWN", "Move down");
        fsm.GetState("L Arrow").AddTransition("UI RIGHT", "Check from left");
        fsm.GetState("R Arrow").AddTransition("UI LEFT", "Check from right");
        UpdateList(0);
    }

    public static void UpdateList(int firstPowerIndex)
    {
        if (firstPowerIndex != -1)
            _inventoryFsm.FsmVariables.FindFsmInt("PowerIndexFirstSlot").Value = firstPowerIndex;
        else
            firstPowerIndex = _inventoryFsm.FsmVariables.FindFsmInt("PowerIndexFirstSlot").Value;
        string[] elementNames =
        [
            FirstPowerSlot,
            SecondPowerSlot,
            ThirdPowerSlot,
            FourthPowerSlot
        ];
        for (int i = 0; i < 4; i++)
        {
            if (firstPowerIndex + i >= CombatController.ObtainedPowers.Count)
                break;
            ElementLookup[elementNames[i]].Item2.text = CombatController.ObtainedPowers[firstPowerIndex + i].Name;
        }
    }

    public static void ResetList()
    {
        _inventoryFsm.FsmVariables.FindFsmInt("PowerIndex").Value = 0;
        string[] elements =
        [
            FirstPowerSlot,
            SecondPowerSlot,
            ThirdPowerSlot,
            FourthPowerSlot,
            PowerName,
            PowerDescription,
            PowerRarity,
            PowerPool,
            PowerScaling
        ];
        foreach (string element in elements)
            ElementLookup[element].Item2.text = "-";
        ElementLookup[MainSprite].Item1.sprite = null;
        ElementLookup[Selector].Item1.transform.localPosition = new(0f, 2.25f);
        UpdateStats();
    }

    public static void UpdateStats()
    {
        ElementLookup[CombatStat].Item2.text = $"<color={CombatController.CombatStatColor}>Combat Level: {CombatController.CombatLevel}</color>";
        ElementLookup[SpiritStat].Item2.text = $"<color={CombatController.SpiritStatColor}>Spirit Level: {CombatController.SpiritLevel}</color>";
        ElementLookup[EnduranceStat].Item2.text = $"<color={CombatController.EnduranceStatColor}>Endurance Level: {CombatController.EnduranceLevel}</color>";
    }

    #endregion

    internal static void Initialize()
    {
        if (_enabled)
            return;
        LogManager.Log("Enabled inventory controller");
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        UpdateList(0);
        UpdateStats();
        _enabled = true;
    }

    internal static void Unload()
    {
        if (!_enabled)
            return;
        LogManager.Log("Disabled inventory controller");
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        ModHooks.LanguageGetHook -= ModHooks_LanguageGetHook;
        ResetList();
        _enabled = false;
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == "ToCInventoryAvailable")
            return true;//return PhaseController.CurrentPhase == Enums.Phase.Run;
        return orig;
    }

    private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
    {
        if (key == "ToCPowers")
            return "Trial Powers";
        else if (key == "ABYSS_MAWLEK_ROYAL")
            return string.Join(", ", Enumerable.Range(0, 9).Select(x => "MOTHWING CLOAK"));
        else if (key == "DREAM_DUMMY")
            return "All arrows point towards me. Not once, not twice, but thrice.";
        else if (key == "ATRIUM_NPC_DREAM_1" && (ScoreController.Score.Mode == Enums.GameMode.Crusader || ScoreController.Score.Mode == Enums.GameMode.GrandCrusader))
        {
            string text = StageController.CurrentRoomData.Last().Name switch
            {
                "GG_Radiance" => "The brightest light shines at the end of the trial.",
                "GG_Grimm_Nightmare" => "Will the crusader be engulfed by the scarlet flames?",
                _ => "The cloud darken in the distance..."
            };
            return text;
        }
        return orig;
    }
}
