using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;
using Caching = TrialOfCrusaders.Powers.Common.Caching;

namespace TrialOfCrusaders;

public static class TreasureManager
{

    private static MethodInfo _invulnerableCall = typeof(HeroController).GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance);

    private static Sprite _backgroundSprite;

    private static GameObject _glow;

    internal static Sprite BackgroundSprite
    {
        get
        {
            _backgroundSprite ??= HeroController.instance.transform.Find("Vignette/Darkness Border/black_solid").GetComponent<SpriteRenderer>().sprite;
            return _backgroundSprite;
        }
    }

    public static Type TestPower => typeof(PolarityShift);

    public static GameObject Shiny { get; set; }

    public static Power[] Powers =>
    [
        // Common
        new StalwartShell(),
        new SoulCatcher(),
        new Sprintmaster(),
        new SteadyBody(),
        new HeavyBlow(),
        new Longnail(),
        new ThornsOfAgony(),
        new BaldurShell(),
        new DefendersCrest(),
        new GlowingWomb(),
        new Sporeshroom(),
        new SharpShadow(),
        new Weaversong(),
        new DreamShield(),
        new GrubberflysElegy(),
        new GatheringSwarm(),
        new DreamNail(),
        new GreatSlash(),
        new CycloneSlash(),
        new DashSlash(),
        new EscapeArtist(),
        new Initiative(),
        new SoulConserver(),
        new Sturdy(),
        new Acrobat(),
        new Recklessness(),
        new Revenge(),
        new DramaticEntrance(),
        new Versatility(),
        new CalmMind(),
        new Perfection(),
        new Cocoon(),
        new BrutalStrikes(),
        new BurstingSoul(),
        new CullTheWeak(),
        new WeakenedHusk(),
        new FocusedEnergy(),
        new GroundSlam(),
        new Fade(),
        new MantisStyle(),
        new Supreme(),
        new CaringShell(),
        new AchillesVerse(),
        new Charge(),
        new Shatter(),
        new BindingCircle(),
        new Interest(),
        new RoyalDecree(),
        new GreaterMind(),
        new DreamPortal(),
        new SpellProdigy(),
        new NailProdigy(),
        new Caching(),
        new Desperation(),
        new Vanish(),
        new Damocles(),
        new PolarityShift(),
        new LifebloodOmen(),
        new DeepBreath(),
        new SeethingLifeblood(),
        // Uncommon
        new SoulEater(),
        new Dashmaster(),
        new Grubsong(),
        new SpellTwister(),
        new MarkOfPride(),
        new FuryOfTheFallen(),
        new QuickFocus(),
        new DeepFocus(),
        new Hiveblood(),
        new ShapeOfUnn(),
        new CarefreeMelody(),
        new Grimmchild(),
        new VengefulSpirit(),
        new DesolateDive(),
        new HowlingWraiths(),
        new Greed(),
        new ImprovedStalwartShell(),
        new ImprovedSprintmaster(),
        new ImprovedGatheringSwarm(),
        new ImprovedHeavyBlow(),
        new ImprovedThornsOfAgony(),
        new ImprovedBaldursShell(),
        new ImprovedDefendersCrest(),
        new ImprovedGlowingWomb(),
        new ImprovedSporeshroom(),
        new ImprovedSharpShadow(),
        new ImprovedWeaversong(),
        new Mindblast(),
        new ImprovedCrystalDash(),
        new ScorchingGround(),
        new ImprovedCaringShell(),
        new CheatDeath(),
        new Pyroblast(),
        new DeepCuts(),
        new FragileGreed(),
        new FragileStrength(),
        new FragileSpirit(),
        new ImprovedMonarchWings(),
        new MercilessPursuit(),
        new HotStreak(),
        // Rare
        new InUtterDarkness(),
        new ShamanStone(),
        new QuickSlash(),
        new Kingsoul(),
        new NailmastersGlory(),
        new DreamWielder(),
        new ShadeSoul(),
        new DescendingDark(),
        new AbyssShriek(),
        new ImprovedSpellTwister(),
        new ImprovedFuryOfTheFallen(),
        new ImprovedHiveblood(),
        new ImprovedCarefreeMelody(),
        new ImprovedGrimmchild(),
        new DreamWalker(),
        new EchoingScream(),
        new ImprovedFocus(),
        new PaleShell(),
        new TreasureHunter(),
        new ShiningBound(),
        new VoidHeart()
    ];

    public static int BadLuckProtection { get; set; } = 0;

    public static bool EnduranceHealthGrant { get; set; }

    public static bool SelectionActive { get; set; }

    internal static void SpawnShiny(TreasureType treasure, Vector3 position, bool fling = true)
    {
        GameObject shiny = UnityEngine.Object.Instantiate(Shiny);
        GameObject glow = null;
        if ((int)treasure > 6 || (int)treasure < 2)
        {
            if (_glow == null)
                _glow = HeroController.instance.transform.Find("Effects/NA Charged").gameObject;
            glow = UnityEngine.Object.Instantiate(_glow, shiny.transform);
            glow.name = "Glow";
            glow.transform.localPosition = new(0f, 0f);
            glow.transform.localScale = new(1, 1f);
            glow.SetActive(true);
        }
        switch (treasure)
        {
            case TreasureType.RareOrb:
            case TreasureType.PrismaticOrb:
                ColorShifter shifter = shiny.AddComponent<ColorShifter>();
                shifter.Rainbox = treasure == TreasureType.RareOrb;
                if (glow != null)
                    shifter.GlowSprite = glow.GetComponent<tk2dSprite>();
                break;
            case TreasureType.CombatOrb:
                shiny.GetComponent<SpriteRenderer>().color = Color.red;
                break;
            case TreasureType.SpiritOrb:
                shiny.GetComponent<SpriteRenderer>().color = new(1f, 0f, 1f, 1f);
                break;
            case TreasureType.EnduranceOrb:
                shiny.GetComponent<SpriteRenderer>().color = Color.green;
                break;
            case TreasureType.Dash:
            case TreasureType.Claw:
            case TreasureType.Wings:
            case TreasureType.Tear:
            case TreasureType.CrystalHeart:
            case TreasureType.ShadeCloak:
            case TreasureType.Lantern:
            case TreasureType.Fireball:
            case TreasureType.Quake:
                shiny.GetComponent<SpriteRenderer>().color = Color.yellow;
                glow.GetComponent<tk2dSprite>().color = Color.yellow;
                break;
            default:
                break;
        }
        shiny.SetActive(true);
        PlayMakerFSM fsm = shiny.LocateMyFSM("Shiny Control");
        fsm.FsmVariables.FindFsmBool("Fling On Start").Value = fling;
        fsm.AddVariable("Item Select", 0);
        fsm.AddVariable("Option 1", "");
        fsm.AddVariable("Option 2", "");
        fsm.AddVariable("Option 3", "");
        var workingState = fsm.GetState("Trink 1");
        workingState.RemoveFirstAction<IncrementPlayerDataInt>();
        workingState.RemoveFirstAction<SetPlayerDataBool>();
        fsm.AddState("Check Choice", () =>
        {
            Transform glow = fsm.transform.Find("Glow");
            if (glow != null)
                UnityEngine.Object.Destroy(glow.gameObject);
            TreasureType treasure = (TreasureType)fsm.FsmVariables.FindFsmInt("Item Select").Value;
            switch (treasure)
            {
                case TreasureType.CombatOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Combat_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_COMBAT";
                    GameHelper.OneTimeMessage("INV_NAME_COMBAT", "Combat Up", "UI");
                    CombatController.CombatLevel++;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.SpiritOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Spirit_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPIRIT";
                    GameHelper.OneTimeMessage("INV_NAME_SPIRIT", "Spirit Up", "UI");
                    CombatController.SpiritLevel++;
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.EnduranceOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Endurance_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_ENDURANCE";
                    GameHelper.OneTimeMessage("INV_NAME_ENDURANCE", "Endurance Up", "UI");
                    CombatController.EnduranceLevel++;
                    EnduranceHealthGrant = true;
                    HeroController.instance.AddHealth(1);
                    EnduranceHealthGrant = false;
                    PlayMakerFSM.BroadcastEvent("MAX HP UP");
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Dash:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Dash_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_DASH";
                    PDHelper.CanDash = true;
                    PDHelper.HasDash = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Claw:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Claw_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_WALLJUMP";
                    PDHelper.HasWalljump = true;
                    PDHelper.CanWallJump = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.CrystalHeart:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Crystal_Dash_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SUPERDASH";
                    PDHelper.HasSuperDash = true;
                    PDHelper.CanSuperDash = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Wings:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Wings_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_DOUBLEJUMP";
                    PDHelper.HasDoubleJump = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Tear:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Tear_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_ACIDARMOUR";
                    PDHelper.HasAcidArmour = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.ShadeCloak:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Shade_Dash_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SHADOWDASH";
                    PDHelper.HasShadowDash = true;
                    PDHelper.CanShadowDash = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Lantern:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Lantern_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_LANTERN";
                    PDHelper.HasLantern = true;
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Fireball:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Vengeful_Spirit_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_FIREBALL";
                    Power fireball = Powers.First(x => x.GetType() == typeof(VengefulSpirit));
                    CombatController.ObtainedPowers.Add(fireball);
                    fireball.EnablePower();
                    break;
                case TreasureType.Quake:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Desolate_Dive_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_QUAKE";
                    Power quake = Powers.First(x => x.GetType() == typeof(DesolateDive));
                    CombatController.ObtainedPowers.Add(quake);
                    quake.EnablePower();
                    break;
                default:
                    int amount = RollSelection(fsm, treasure);
                    TrialOfCrusaders.Holder.StartCoroutine(SelectPower(CreatePowerOverlay(fsm, amount), fsm, amount));
                    break;
            }
        }, FsmTransitionData.FromTargetState("Flash").WithEventName("CHARM"),
            FsmTransitionData.FromTargetState("Trink Flash").WithEventName("TRINKET"));
        fsm.GetState("Charm?").AdjustTransitions("Check Choice");
        fsm.GetState("Trink Flash").AdjustTransitions("Trink 1");
        shiny.transform.position = position;
        fsm.FsmVariables.FindFsmInt("Item Select").Value = (int)treasure;
        fsm.GetState("Hero Down").InsertActions(0, () => SelectionActive = true);
        fsm.GetState("Finish").AddActions(() =>
        {
            SelectionActive = false;
            // To prevent situation where the player takes unavoidable damage, we grant 2 seconds of invincibility.
            if (!StageController.QuietRoom)
                HeroController.instance.StartCoroutine((IEnumerator)_invulnerableCall.Invoke(HeroController.instance, [2f]));
        });
    }

    private static int RollSelection(PlayMakerFSM fsm, TreasureType treasureType)
    {
        if (treasureType == TreasureType.NormalOrb || treasureType == TreasureType.RareOrb)
        {
            bool rare = false;
            List<Power> selectedPowers = [];
            List<string> statBoni = [];
            List<Power> availablePowers = [.. Powers.Except(CombatController.ObtainedPowers)
                .Where(x => x.CanAppear)];
            for (int i = 0; i < 3; i++)
            {
                if (availablePowers.Count == 0)
                    break;
                else
                {
                    List<Power> powerPool = [.. availablePowers];
                    int rolledNumber = treasureType == TreasureType.RareOrb
                        ? 99
                        : RngProvider.GetStageRandom(1, 100);
                    if (rolledNumber <= 65 - BadLuckProtection)
                        powerPool = [.. powerPool.Where(x => x.Tier == Rarity.Common)];
                    else if (rolledNumber <= 95 - BadLuckProtection)
                        powerPool = [.. powerPool.Where(x => x.Tier == Rarity.Uncommon)];
                    else
                    {
                        powerPool = [.. powerPool.Where(x => x.Tier == Rarity.Rare)];
                        if (treasureType == TreasureType.NormalOrb)
                            BadLuckProtection = 0;
                        rare = true;
                    }
                    // If none is available in this pool, reroll.
                    // ToDo: Force a cleaner solution
                    if (powerPool.Count == 0)
                    {
                        i--;
                        continue;
                    }
                    selectedPowers.Add(powerPool[RngProvider.GetStageRandom(0, powerPool.Count - 1)]);
                }
                availablePowers.Remove(selectedPowers.Last());
                Power selectedPower = selectedPowers.Last();
                int rolledBonus = RngProvider.GetStageRandom(1, 100);
                if (rolledBonus <= selectedPower.BonusRates.Item1 && CombatController.CombatLevel < 20)
                    statBoni.Add("Combat");
                else if (rolledBonus <= selectedPower.BonusRates.Item1 + selectedPower.BonusRates.Item2 && CombatController.SpiritLevel < 20)
                    statBoni.Add("Spirit");
                else if (rolledBonus <= selectedPower.BonusRates.Item1 + selectedPower.BonusRates.Item2 + selectedPower.BonusRates.Item3 && CombatController.EnduranceLevel < 20)
                    statBoni.Add("Endurance");
                else
                    statBoni.Add(null);
            }
            if (selectedPowers.Count != 0)
            {
                if (!rare)
                    BadLuckProtection = Math.Min(BadLuckProtection + 8, 64);
                for (int i = 0; i < selectedPowers.Count; i++)
                    fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value = string.IsNullOrEmpty(statBoni[i])
                            ? selectedPowers[i].Name
                            : $"{selectedPowers[i].Name}_{statBoni[i]}";
                return selectedPowers.Count;
            }
        }

        int optionAmount = 3;
        List<string> options = [];
        if (CombatController.CombatLevel != 20)
            options.Add("Combat");
        if (CombatController.SpiritLevel != 20)
            options.Add("Spirit");
        if (CombatController.EnduranceLevel != 20)
            options.Add("Endurance");
        if (treasureType == TreasureType.CatchUpStat)
        {
            int[] amounts = [CombatController.CombatLevel, CombatController.SpiritLevel, CombatController.EnduranceLevel];
            int max = amounts.Max();
            if (amounts.Count(x => x == max) == 1)
            {
                optionAmount = 2;
                if (CombatController.CombatLevel == max)
                    options.Remove("Combat");
                else if (CombatController.SpiritLevel == max)
                    options.Remove("Spirit");
                else
                    options.Remove("Endurance");
            }
        }
        // Fill the rest with just geo.
        while (options.Count < optionAmount)
            options.Add("Geo");
        for (int i = 0; i < optionAmount; i++)
            fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value = options[i];
        return optionAmount;
    }

    #region Selection Handling

    //private static int _powerSet = 32;

    internal static GameObject CreatePowerOverlay(PlayMakerFSM fsm, int optionAmount)
    {
        GameObject powerOverlay = new("Power overlay");
        GameObject viewBlocker = new("View Blocker");
        viewBlocker.transform.SetParent(powerOverlay.transform, true);
        viewBlocker.AddComponent<SpriteRenderer>().sprite = BackgroundSprite;
        viewBlocker.GetComponent<SpriteRenderer>().sortingOrder = 1;
        viewBlocker.GetComponent<SpriteRenderer>().color = new(0f, 0f, 0f, 0.9f);
        viewBlocker.transform.localScale = new Vector3(5000f, 5000f, 5000f);
        viewBlocker.SetActive(true);
        int layer = 0;

        GameObject prefab = GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Inv/Inv_Items/Geo").gameObject;
        SpawnStatBox(prefab, powerOverlay.transform);
        for (int i = 0; i < optionAmount; i++)
            layer = CreateOption(i, powerOverlay.transform, prefab, fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value);
        CreateArrows(powerOverlay.transform, layer);

        GameObject titleText = UnityEngine.Object.Instantiate(prefab, powerOverlay.transform, true);
        titleText.name = "Title";
        titleText.transform.position = new(0f, 6.8f, 0f);
        titleText.transform.localScale = new(2f, 2f);
        TextMeshPro text = titleText.GetComponent<DisplayItemAmount>().textObject;
        UnityEngine.Object.Destroy(titleText.GetComponent<DisplayItemAmount>());
        UnityEngine.Object.Destroy(titleText.GetComponent<SpriteRenderer>());
        text.fontSize = 3f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"Select an upgrade!";
        text.alignment = TextAlignmentOptions.Center;
        text.textContainer.size = new(5f, 1f);
        powerOverlay.SetActive(true);
        //_powerSet++;
        //if (_powerSet == 40)
        //    LogHelper.Write("Called last power set.");
        return powerOverlay;
    }

    private static void SpawnStatBox(GameObject prefab, Transform parent)
    {
        GameObject statInfo = UnityEngine.Object.Instantiate(prefab, parent, true);
        statInfo.name = "Stat";
        statInfo.transform.position = new(-13.5f, 2f, 0f);
        statInfo.transform.localScale = new(2f, 2f);
        TextMeshPro text = statInfo.GetComponent<DisplayItemAmount>().textObject;
        UnityEngine.Object.Destroy(statInfo.GetComponent<DisplayItemAmount>());
        UnityEngine.Object.Destroy(statInfo.GetComponent<SpriteRenderer>());
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color=#fa0000>Combat: {CombatController.CombatLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statInfo = UnityEngine.Object.Instantiate(prefab, parent, true);
        statInfo.name = "Stat";
        statInfo.transform.position = new(-13.5f, 1.3f, 0f);
        statInfo.transform.localScale = new(2f, 2f);
        text = statInfo.GetComponent<DisplayItemAmount>().textObject;
        UnityEngine.Object.Destroy(statInfo.GetComponent<DisplayItemAmount>());
        UnityEngine.Object.Destroy(statInfo.GetComponent<SpriteRenderer>());
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color=#a700fa>Spirit: {CombatController.SpiritLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statInfo = UnityEngine.Object.Instantiate(prefab, parent, true);
        statInfo.name = "Stat";
        statInfo.transform.position = new(-13.5f, 0.6f, 0f);
        statInfo.transform.localScale = new(2f, 2f);
        text = statInfo.GetComponent<DisplayItemAmount>().textObject;
        UnityEngine.Object.Destroy(statInfo.GetComponent<DisplayItemAmount>());
        UnityEngine.Object.Destroy(statInfo.GetComponent<SpriteRenderer>());
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color=#4fff61>Endurance: {CombatController.EnduranceLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statInfo = new("Upper stat boarder");
        statInfo.layer = 5; // UI
        statInfo.transform.SetParent(parent);
        statInfo.transform.position = new(-14.3f, 1.8f);
        statInfo.transform.localScale = new(0.5f, 0.5f);
        statInfo.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Stat_Border_Upper");

        statInfo = new("Lower stat boarder");
        statInfo.layer = 5; // UI
        statInfo.transform.SetParent(parent);
        statInfo.transform.position = new(-14.3f, -0.8f);
        statInfo.transform.localScale = new(0.5f, 0.5f);
        statInfo.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Stat_Border_Lower");
    }

    private static int CreateOption(int count, Transform parent, GameObject prefab, string optionName)
    {
        GameObject option = UnityEngine.Object.Instantiate(prefab, parent, true);
        option.name = "Option " + (count + 1);
        option.transform.position = new(-6.2f + count * 8.5f, 2f, 0f);
        option.transform.localScale = new(2f, 2f);
        TextMeshPro titleText = option.GetComponent<DisplayItemAmount>().textObject;
        UnityEngine.Object.Destroy(option.GetComponent<DisplayItemAmount>());
        titleText.fontSize = 2.5f;
        titleText.enableWordWrapping = true;
        titleText.gameObject.name = "Power Name";
        titleText.textContainer.size = new(3f, 2f);
        titleText.alignment = TextAlignmentOptions.Center;
        option.transform.GetChild(0).localPosition = new(0f, -2f);
        GameObject text = UnityEngine.Object.Instantiate(titleText.gameObject, option.transform);
        TextMeshPro description = text.GetComponent<TextMeshPro>();
        description.textContainer.size = new(2.5f, 5f);
        description.alignment = TextAlignmentOptions.Center;
        description.fontSize = 2f;
        description.enableWordWrapping = true;
        description.transform.localPosition = new(0f, -3.5f);

        string powerName = optionName.Contains("_")
            ? optionName.Split('_')[0]
            : optionName;
        Power selectedPower = Powers.FirstOrDefault(x => x.Name == powerName);
        //Power selectedPower = Powers[count + _powerSet * 3];
        if (selectedPower != null)
        {
            titleText.text = selectedPower.Name;
            description.text = selectedPower.Description;
            option.GetComponent<SpriteRenderer>().sprite = selectedPower.Sprite;

            GameObject rarity = UnityEngine.Object.Instantiate(titleText.gameObject, option.transform);
            TextMeshPro rarityText = rarity.GetComponent<TextMeshPro>();
            rarityText.text = $"({selectedPower.Tier})";
            rarityText.color = selectedPower.Tier switch
            {
                Rarity.Common => Color.white,
                Rarity.Uncommon => new(0.2f, 0.8f, 0.2f), // Lime
                _ => Color.cyan
            };
            rarityText.textContainer.size = new(2f, 2f);
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.fontSize = 1.5f;
            rarityText.transform.localPosition = new(0f, -1.45f);

            if (optionName.Contains("_"))
            {
                string bonusStat = optionName.Split('_')[1];
                GameObject bonus = UnityEngine.Object.Instantiate(titleText.gameObject, option.transform);
                TextMeshPro bonusText = bonus.GetComponent<TextMeshPro>();
                bonusText.text = bonusStat switch
                {
                    "Combat" => "+1 Combat",
                    "Spirit" => "+1 Spirit",
                    _ => "+1 Endurance"
                };
                bonusText.color = bonusStat switch
                {
                    "Combat" => Color.red,
                    "Spirit" => new(0.5f, 0f, 0.5f),
                    _ => Color.green
                };
                bonusText.textContainer.size = new(2f, 2f);
                bonusText.alignment = TextAlignmentOptions.Center;
                bonusText.fontSize = 2;
                bonusText.transform.localPosition = new(0f, -4.8f);
            }
        }
        else
        {
            titleText.text = powerName switch
            {
                "Combat" => "Combat Up",
                "Spirit" => "Spirit Up",
                "Endurance" => "Endurance Up",
                _ => "200 Geo"
            };
            description.text = powerName switch
            {
                "Combat" => "Increases your nail damage. Abilities marked with (C) scale with your combat level.",
                "Spirit" => "Increases the amount of soul you can have. Abilities marked with (S) scale with your spirit level.",
                "Endurance" => "Increases your maximum health. Abilities marked with (E) scale with your endurance level.",
                _ => "If nothing else, geo is always there."
            };
            option.GetComponent<SpriteRenderer>().sprite = powerName switch
            {
                "Combat" => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Combat_Icon"),
                "Spirit" => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Spirit_Icon"),
                "Endurance" => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Endurance_Icon"),
                _ => SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Abilities.Placeholder")
            };
        }

        GameObject statInfo = new("Ability Border");
        statInfo.layer = 5; // UI
        statInfo.transform.SetParent(option.transform);
        statInfo.transform.position = new(-6.2f + count * 8.5f, 4.7f);
        statInfo.transform.localScale = new(0.8f, 0.8f);
        statInfo.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Power_Border");

        statInfo = new("Ability Border");
        statInfo.layer = 5; // UI
        statInfo.transform.SetParent(option.transform);
        statInfo.transform.position = new(-6.2f + count * 8.5f, -8.2f);
        statInfo.transform.localScale = new(0.8f, 0.8f);
        statInfo.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Power_Border");

        option.SetActive(true);
        return prefab.layer;
    }

    private static void CreateArrows(Transform parent, int layer)
    {
        GameObject rotateLeftArrow = new("MoveLeft");
        rotateLeftArrow.transform.SetParent(parent);
        rotateLeftArrow.transform.localPosition = new(-9f, 1.7f);
        rotateLeftArrow.transform.localScale = new(3f, 3f);
        rotateLeftArrow.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
        rotateLeftArrow.GetComponent<SpriteRenderer>().flipX = true;
        rotateLeftArrow.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
        rotateLeftArrow.SetActive(true);

        GameObject rotateRightArrow = new("MoveRight");
        rotateRightArrow.transform.SetParent(parent);
        rotateRightArrow.transform.localPosition = new(-3.4f, 1.7f);
        rotateRightArrow.transform.localScale = new(3f, 3f);
        rotateRightArrow.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Arrow");
        rotateRightArrow.GetComponent<SpriteRenderer>().sortingLayerID = 629535577;
        rotateRightArrow.SetActive(true);

        rotateLeftArrow.layer = layer;
        rotateRightArrow.layer = layer;
    }

    private static IEnumerator SelectPower(GameObject powerSelector, PlayMakerFSM shinyFsm, int amount)
    {
        GameObject leftArrow = powerSelector.transform.Find("MoveLeft").gameObject;
        GameObject rightArrow = powerSelector.transform.Find("MoveRight").gameObject;
        int powerSlot = 0;
        bool selected = false;
        bool inputPause = false;
        while (!selected)
        {
            yield return null;
            if (InputHandler.Instance.inputActions.jump.IsPressed || InputHandler.Instance.inputActions.menuSubmit.IsPressed)
                selected = true;
            else if (InputHandler.Instance.inputActions.left.IsPressed)
            {
                powerSlot--;
                if (powerSlot <= -1)
                    powerSlot = amount - 1;
                inputPause = true;
            }
            else if (InputHandler.Instance.inputActions.right.IsPressed)
            {
                powerSlot++;
                if (powerSlot >= amount)
                    powerSlot = 0;
                inputPause = true;
            }
            leftArrow.transform.localPosition = new(-9f + powerSlot * 8.5f, 1.7f);
            rightArrow.transform.localPosition = new(-3.4f + powerSlot * 8.5f, 1.7f);
            if (inputPause)
            {
                yield return new WaitForSeconds(0.3f);
                inputPause = false;
            }
        }
        UnityEngine.Object.Destroy(leftArrow.transform.parent.gameObject);
        string pickedValue = shinyFsm.FsmVariables.FindFsmString("Option " + (powerSlot + 1)).Value;
        Power pickedPower = null;
        string pickedStat = null;
        if (pickedValue.Contains("_"))
        {
            pickedPower = Powers.FirstOrDefault(x => x.Name == pickedValue.Split('_')[0]);
            pickedStat = pickedValue.Split('_')[1];
        }
        else
            pickedPower = Powers.FirstOrDefault(x => x.Name == pickedValue);

        if (pickedPower == null)
            pickedStat = pickedValue;
        if (pickedStat != null)
            switch (pickedStat)
            {
                case "Combat":
                    CombatController.CombatLevel++;
                    PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
                    break;
                case "Spirit":
                    CombatController.SpiritLevel++;
                    break;
                case "Endurance":
                    CombatController.EnduranceLevel++;
                    EnduranceHealthGrant = true;
                    HeroController.instance.AddHealth(1);
                    EnduranceHealthGrant = false;
                    PlayMakerFSM.BroadcastEvent("MAX HP UP");
                    break;
                default:
                    HeroController.instance.AddGeo(200);
                    break;
            }
        if (pickedPower != null)
        {
            CombatController.ObtainedPowers.Add(pickedPower);
            pickedPower.EnablePower();
        }
        shinyFsm.SendEvent("CHARM");
        EventRegister.SendEvent("ADD BLUE HEALTH");
        if (StageController.CurrentRoomNumber >= 1 && StageController.CurrentRoom.BossRoom && !StageController.QuietRoom)
            TrialOfCrusaders.Holder.StartCoroutine(StageController.WaitForTransition());
    }

    #endregion
}
