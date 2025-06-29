using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;
using Caching = TrialOfCrusaders.Powers.Common.Caching;

namespace TrialOfCrusaders.Manager;

public static class TreasureManager
{
    internal static string UncommonTextColor = "#33cc33";
    internal static string RareTextColor = "#00ffff";

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

    public static GameObject BigItemUI { get; set; }

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
        new IntimidatingShout(),
        new Versatility(),
        new CalmMind(),
        new Perfection(),
        new Cocoon(),
        new BrutalStrikes(),
        new BurstingSoul(),
        new WeakenedHusk(),
        new ChannelledEnergy(),
        new GroundSlam(),
        new Fade(),
        new MantisStyle(),
        new Supreme(),
        new CaringShell(),
        new AchillesHeel(),
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
        new DeepBreath(),
        new SeethingLifeblood(),
        new Hiveblood(),
        new SoulEater(),
        new Dashmaster(),
        new Grubsong(),
        new SpellTwister(),
        new MarkOfPride(),
        new FuryOfTheFallen(),
        new QuickFocus(),
        new DeepFocus(),
        new CullTheWeak(),
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
        new LuckyCharm(),
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
        new Mediocracy(),
        new WaywardCompass(),
        new VoidHeart(),
    ];

    public static int BadLuckProtection { get; set; } = 0;

    public static bool EnduranceHealthGrant { get; set; }

    public static bool SelectionActive { get; set; }

    public static int SelectionCount { get; set; }

    public static event Action<TreasureType, GameObject> SpawnedShiny;

    public static event Action<Power> PowerSelected;

    internal static void SetupShiny(GameObject chest)
    {
        Shiny = chest.transform.Find("Item").GetChild(0).gameObject;
        Shiny.name = "ToC Item";
        UnityEngine.Object.Destroy(Shiny.GetComponent<ObjectBounce>());
        UnityEngine.Object.Destroy(Shiny.GetComponent<PersistentBoolItem>());
        UnityEngine.Object.DontDestroyOnLoad(Shiny);

        BigItemUI = GameObject.Instantiate(Shiny.LocateMyFSM("Shiny Control").GetState("Dash").GetFirstAction<CreateUIMsgGetItem>().gameObject.Value);
        BigItemUI.SetActive(false);
        GameObject.DontDestroyOnLoad(BigItemUI);
    }

    internal static GameObject SpawnShiny(TreasureType treasure, Vector3 position, bool fling = true)
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
                shifter.Rainbow = treasure == TreasureType.RareOrb;
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
            case TreasureType.StashedContraband:
            case TreasureType.Toughness:
            case TreasureType.Highroller:
            case TreasureType.Archive:
                shiny.GetComponent<SpriteRenderer>().color = Color.cyan;
                glow.GetComponent<tk2dSprite>().color = Color.cyan;
                break;
            default:
                break;
        }
        PlayMakerFSM fsm = shiny.LocateMyFSM("Shiny Control");
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
            SelectionCount++;
            TreasureType treasure = (TreasureType)fsm.FsmVariables.FindFsmInt("Item Select").Value;
            switch (treasure)
            {
                case TreasureType.CombatOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Combat_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_COMBAT";
                    GameHelper.OneTimeMessage("INV_NAME_COMBAT", "Combat Up", "UI");
                    GrantCombatLevel();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.SpiritOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Spirit_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPIRIT";
                    GameHelper.OneTimeMessage("INV_NAME_SPIRIT", "Spirit Up", "UI");
                    GrantSpiritLevel();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.EnduranceOrb:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Endurance_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_ENDURANCE";
                    GameHelper.OneTimeMessage("INV_NAME_ENDURANCE", "Endurance Up", "UI");
                    GrantEnduranceLevel();
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
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPELL_FIREBALL1";
                    Power fireball = Powers.First(x => x.GetType() == typeof(VengefulSpirit));
                    CombatController.ObtainedPowers.Add(fireball);
                    fireball.EnablePower();
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Quake:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Desolate_Dive_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPELL_QUAKE1";
                    Power quake = Powers.First(x => x.GetType() == typeof(DesolateDive));
                    CombatController.ObtainedPowers.Add(quake);
                    quake.EnablePower();
                    StageController.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.StashedContraband:
                case TreasureType.Toughness:
                case TreasureType.Highroller:
                case TreasureType.Archive:
                    fsm.SendEvent("BIG");
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
        fsm.GetState("Hero Down").InsertActions(0, () => SelectionActive = true);
        fsm.GetState("Finish").AddActions(() =>
        {
            SelectionActive = false;
            // To prevent situation where the player takes unavoidable damage, we grant 2 seconds of invincibility.
            if (!StageController.QuietRoom)
                HeroController.instance.StartCoroutine((IEnumerator)_invulnerableCall.Invoke(HeroController.instance, [2f]));
        });
        fsm.GetState("Destroy").RemoveAllActions();
        fsm.GetState("Destroy").AddTransition("FINISHED", "Fling?");

        if ((int)treasure > 15)
        {
            fsm.GetState("Check Choice").AddTransition("BIG", "Big Get Flash");
            fsm.AddState("Show major item", () =>
            {
                var bigUI = GameObject.Instantiate(BigItemUI, Vector3.zero, Quaternion.identity);
                bigUI.SetActive(true);
                PlayMakerFSM fsm = bigUI.LocateMyFSM("Msg Control");
                fsm.AddState("Prepare Item", () =>
                {
                    SecretController.SetupItemScreen(fsm, treasure);
                }, FsmTransitionData.FromTargetState("Audio Player Actor").WithEventName("FINISHED"));
                fsm.GetState("Init").AddTransition("FINISHED", "Prepare Item");
            }, FsmTransitionData.FromTargetState("Trink Pause").WithEventName("GET ITEM MSG END"));
            fsm.GetState("Big Get Flash").AdjustTransitions("Show major item");
        }
        shiny.SetActive(true);
        shiny.AddComponent<LeftShinyFlag>().Treasure = treasure;
        fsm.FsmVariables.FindFsmBool("Fling On Start").Value = fling;
        shiny.transform.position = position;
        fsm.FsmVariables.FindFsmInt("Item Select").Value = (int)treasure;
        SpawnedShiny?.Invoke(treasure, shiny);
        return shiny;
    }

    private static int RollSelection(PlayMakerFSM fsm, TreasureType treasureType)
    {
        if (treasureType == TreasureType.NormalOrb || treasureType == TreasureType.RareOrb)
        {
            bool rare = false;
            List<Power> selectedPowers = [];
            List<string> statBoni = [];
            List<string> availablePowerNames = [.. Powers.Where(x => x.CanAppear).Select(x => x.Name)];
            List<string> obtainedPowerNames = [.. CombatController.ObtainedPowers.Select(x => x.Name)];
            availablePowerNames = [.. availablePowerNames.Except(obtainedPowerNames)];
            List<Power> availablePowers = [];
            foreach (string powerName in availablePowerNames)
                availablePowers.Add(Powers.First(x => x.Name == powerName));

            for (int i = 0; i < 3; i++)
            {
                if (availablePowers.Count == 0)
                    break;
                else
                {
                    List<Power> powerPool = [.. availablePowers];
                    int rolledNumber = treasureType == TreasureType.RareOrb
                        ? 99
                        : RngManager.GetRandom(1, 100);
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
                    // Force treasure test code.
                    //if (i == 0)
                    //    selectedPowers.Add(Powers.First(x => x.GetType() == typeof(Grimmchild)));
                    //else if (i == 1)
                    //    selectedPowers.Add(Powers.First(x => x.GetType() == typeof(Weaversong)));
                    //else
                        selectedPowers.Add(powerPool[RngManager.GetRandom(0, powerPool.Count - 1)]);
                }
                availablePowers.Remove(selectedPowers.Last());
                Power selectedPower = selectedPowers.Last();
                (float, float, float) bonusChances = selectedPower.BonusRates;
                int rolledBonus = RngManager.GetRandom(1, 100);
                if (selectedPower.Tier != Rarity.Rare && CombatController.HasPower<LuckyCharm>(out _))
                    bonusChances = new(bonusChances.Item1 * 1.5f, bonusChances.Item2 * 1.5f, bonusChances.Item3 * 2);
                if (rolledBonus <= bonusChances.Item1 && !CombatController.CombatCapped)
                    statBoni.Add("Combat");
                else if (rolledBonus <= bonusChances.Item1 + bonusChances.Item2 && !CombatController.SpiritCapped)
                    statBoni.Add("Spirit");
                else if (rolledBonus <= bonusChances.Item1 + bonusChances.Item2 + bonusChances.Item3 && !CombatController.EnduranceCapped)
                    statBoni.Add("Endurance");
                else
                    statBoni.Add(null);
            }
            if (selectedPowers.Count != 0)
            {
                if (!rare)
                    BadLuckProtection = Math.Min(BadLuckProtection + 4, 64);
                for (int i = 0; i < selectedPowers.Count; i++)
                    fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value = string.IsNullOrEmpty(statBoni[i])
                            ? selectedPowers[i].Name
                            : $"{selectedPowers[i].Name}_{statBoni[i]}";
                return selectedPowers.Count;
            }
        }

        int optionAmount = 3;
        List<string> options = [];
        if (!CombatController.CombatCapped)
            options.Add("Combat");
        if (!CombatController.SpiritCapped)
            options.Add("Spirit");
        if (!CombatController.EnduranceCapped)
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

        SpawnStatBox(powerOverlay.transform);
        for (int i = 0; i < optionAmount; i++)
            layer = CreateOption(i, powerOverlay.transform, fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value);
        CreateArrows(powerOverlay.transform, layer);

        CreateForfeitOption(powerOverlay.transform);

        TreasureType selectionType = (TreasureType)fsm.FsmVariables.FindFsmInt("Item Select").Value;
        if (selectionType == TreasureType.NormalOrb || selectionType == TreasureType.RareOrb)
            CreateRerollOption(powerOverlay.transform);

        (SpriteRenderer, TextMeshPro) titleInfo = TextManager.CreateUIObject("Title");
        GameObject titleText = titleInfo.Item1.gameObject;
        titleText.transform.SetParent(powerOverlay.transform);
        titleText.transform.position = new(0f, 6.8f, 0f);
        titleText.transform.localScale = new(2f, 2f);
        UnityEngine.Object.Destroy(titleInfo.Item1);
        TextMeshPro text = titleInfo.Item2;
        text.fontSize = 3f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"Select an upgrade!";
        text.alignment = TextAlignmentOptions.Center;
        text.textContainer.size = new(5f, 1f);
        powerOverlay.SetActive(true);
        //_powerSet++;
        //if (_powerSet == 40)
        //    LogManager.Log("Called last power set.");
        return powerOverlay;
    }

    private static void SpawnStatBox(Transform parent)
    {
        (SpriteRenderer, TextMeshPro) statInfo = TextManager.CreateUIObject("Combat Stat");
        GameObject statObject = statInfo.Item1.gameObject;
        statObject.transform.SetParent(parent);
        statObject.transform.position = new(-13.5f, 2f, 0f);
        statObject.transform.localScale = new(2f, 2f);
        UnityEngine.Object.Destroy(statInfo.Item1);
        TextMeshPro text = statInfo.Item2;
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color={CombatController.CombatStatColor}>Combat: {CombatController.CombatLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statInfo = TextManager.CreateUIObject("Spirit Stat");
        statObject = statInfo.Item1.gameObject;
        statObject.transform.SetParent(parent);
        statObject.transform.position = new(-13.5f, 1.3f, 0f);
        statObject.transform.localScale = new(2f, 2f);
        UnityEngine.Object.Destroy(statInfo.Item1);
        text = statInfo.Item2;
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color={CombatController.SpiritStatColor}>Spirit: {CombatController.SpiritLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statInfo = TextManager.CreateUIObject("Endurance Stat");
        statObject = statInfo.Item1.gameObject;
        statObject.transform.SetParent(parent);
        statObject.transform.position = new(-13.5f, 0.6f, 0f);
        statObject.transform.localScale = new(2f, 2f);
        UnityEngine.Object.Destroy(statInfo.Item1);
        text = statInfo.Item2;
        text.fontSize = 2f;
        text.enableWordWrapping = true;
        text.textContainer.size = new(3f, 1f);
        text.text = $"<color={CombatController.EnduranceStatColor}>Endurance: {CombatController.EnduranceLevel}</color>";
        text.alignment = TextAlignmentOptions.Left;

        statObject = new("Upper stat boarder")
        {
            layer = 5 // UI
        };
        statObject.transform.SetParent(parent);
        statObject.transform.position = new(-14.3f, 1.8f);
        statObject.transform.localScale = new(0.5f, 0.5f);
        statObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Stat_Border_Upper");

        statObject = new("Lower stat boarder")
        {
            layer = 5 // UI
        };
        statObject.transform.SetParent(parent);
        statObject.transform.position = new(-14.3f, -0.8f);
        statObject.transform.localScale = new(0.5f, 0.5f);
        statObject.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Border.Stat_Border_Lower");
    }

    private static int CreateOption(int count, Transform parent, string optionName)
    {
        (SpriteRenderer, TextMeshPro) optionPair = TextManager.CreateUIObject("Option " + (count + 1));
        GameObject option = optionPair.Item1.gameObject;
        option.transform.SetParent(parent);
        option.transform.position = new(-6.2f + count * 8.5f, 2f, 0f);
        option.transform.localScale = new(2f, 2f);
        TextMeshPro titleText = optionPair.Item2;
        titleText.fontSize = 2.5f;
        titleText.enableWordWrapping = true;
        titleText.name = "Power Title";
        titleText.textContainer.size = new(3f, 2f);
        titleText.alignment = TextAlignmentOptions.Center;
        option.transform.GetChild(0).localPosition = new(0f, -2f);

        (SpriteRenderer, TextMeshPro) descriptionObject = TextManager.CreateUIObject("Description");
        descriptionObject.Item2.transform.SetParent(option.transform);
        UnityEngine.Object.Destroy(descriptionObject.Item1.gameObject);
        TextMeshPro description = descriptionObject.Item2;
        description.textContainer.size = new(2.5f, 5f);
        description.alignment = TextAlignmentOptions.Center;
        description.fontSize = 2f;
        description.enableWordWrapping = true;
        description.transform.localPosition = new(0f, -3.5f);

        string powerName = optionName.Contains("_")
            ? optionName.Split('_')[0]
            : optionName;
        Power selectedPower = Powers.FirstOrDefault(x => x.Name == powerName);
        if (selectedPower != null)
        {
            titleText.text = selectedPower.ScaledName;
            description.text = selectedPower.Description;
            option.GetComponent<SpriteRenderer>().sprite = selectedPower.Sprite;

            (SpriteRenderer, TextMeshPro) rarityInfo = TextManager.CreateUIObject("Rarity");
            GameObject rarity = rarityInfo.Item2.gameObject;
            rarity.transform.SetParent(option.transform);
            UnityEngine.Object.Destroy(rarityInfo.Item1.gameObject);
            TextMeshPro rarityText = rarityInfo.Item2;
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
                (SpriteRenderer, TextMeshPro) bonusInfo = TextManager.CreateUIObject("Bonus");
                GameObject bonus = bonusInfo.Item2.gameObject;
                bonus.transform.SetParent(option.transform);
                UnityEngine.Object.Destroy(bonusInfo.Item1.gameObject);
                TextMeshPro bonusText = bonusInfo.Item2;
                bonusText.text = bonusStat switch
                {
                    "Combat" => "+1 Combat",
                    "Spirit" => "+1 Spirit",
                    _ => "+1 Endurance"
                };
                bonusText.color = bonusStat switch
                {
                    "Combat" => Color.red,
                    "Spirit" => new(0.957f, 0.012f, 0.988f),
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
                "Spirit" => "Increases the amount of soul you can have and receive. Also increases spell damage. Abilities marked with (S) scale with your spirit level.",
                "Endurance" => "Increases your maximum and current health. Abilities marked with (E) scale with your endurance level.",
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
        return optionPair.Item1.gameObject.layer;
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

    private static void CreateForfeitOption(Transform parent)
    {
        (SpriteRenderer, TextMeshPro) optionPair = TextManager.CreateUIObject("Forfeit power");
        GameObject option = optionPair.Item2.gameObject;
        option.transform.SetParent(parent);
        GameObject.Destroy(optionPair.Item1);
        optionPair.Item2.text = "Forfeit selection";
        option.transform.position = new(-13.9f, -6f);
        optionPair.Item2.fontSize = 3;
    }

    private static void CreateRerollOption(Transform parent)
    {
        if (!SecretController.UnlockedHighRoller || SecretController.LeftRolls <= 0)
            return;
        (SpriteRenderer, TextMeshPro) optionPair = TextManager.CreateUIObject("Reroll power");
        GameObject option = optionPair.Item2.gameObject;
        option.transform.SetParent(parent);
        GameObject.Destroy(optionPair.Item1);
        optionPair.Item2.text = $"Reroll ({SecretController.LeftRolls} left)";
        option.transform.position = new(-13.9f, -5f);
        optionPair.Item2.fontSize = 3;
    }

    private static IEnumerator SelectPower(GameObject powerSelector, PlayMakerFSM shinyFsm, int amount)
    {
        // To prevent insta selections.
        yield return new WaitUntil(() => !InputHandler.Instance.inputActions.jump.IsPressed && !InputHandler.Instance.inputActions.menuSubmit.IsPressed);
        GameObject leftArrow = powerSelector.transform.Find("MoveLeft").gameObject;
        GameObject rightArrow = powerSelector.transform.Find("MoveRight").gameObject;
        int powerSlot = 0;
        bool selected = false;
        bool inputPause = false;
        bool canReroll = SecretController.LeftRolls > 0 && shinyFsm.FsmVariables.FindFsmInt("Item Select").Value < 2;
        while (!selected)
        {
            yield return null;
            if (InputHandler.Instance.inputActions.jump.IsPressed || InputHandler.Instance.inputActions.menuSubmit.IsPressed)
                selected = true;
            else if (InputHandler.Instance.inputActions.left.IsPressed)
            {
                powerSlot--;
                if (powerSlot == -3 || (!canReroll && powerSlot == -2))
                    powerSlot = amount - 1;
                inputPause = true;
            }
            else if (InputHandler.Instance.inputActions.right.IsPressed)
            {
                powerSlot++;
                if (powerSlot >= amount)
                    powerSlot = canReroll ? -2 : -1;
                inputPause = true;
            }
            if (powerSlot >= 0)
            {
                leftArrow.transform.localPosition = new(-9f + powerSlot * 8.5f, 1.7f);
                rightArrow.transform.localPosition = new(-3.4f + powerSlot * 8.5f, 1.7f);
                leftArrow.transform.localScale = new(3f, 3f);
                rightArrow.transform.localScale = new(3f, 3f);

                if (!SecretController.UnlockedHighRoller)
                    if (SelectionCount == 3 || SelectionCount == 7 || SelectionCount == 12 || SelectionCount == 18)
                    {
                        leftArrow.transform.SetRotation2D(0);
                        rightArrow.transform.SetRotation2D(0);
                        if (SelectionCount == 3 && powerSlot == 1)
                            leftArrow.transform.SetRotation2D(180);
                        else if (SelectionCount == 18 && powerSlot == 0)
                            rightArrow.transform.SetRotation2D(180);
                        else if (SelectionCount == 7 && powerSlot == 2)
                            leftArrow.transform.SetRotation2D(90);
                        else if (SelectionCount == 12 && powerSlot == 0)
                            rightArrow.transform.SetRotation2D(90);
                    }
                    else
                        leftArrow.GetComponent<SpriteRenderer>().flipX = true;
            }
            else
            {
                leftArrow.transform.localPosition = new(-15f, powerSlot == -2 ? -4.2f : -5.2f);
                rightArrow.transform.localPosition = new(-9.25f, powerSlot == -2 ? -4.2f : -5.2f);
                leftArrow.transform.localScale = new(1f, 1f);
                rightArrow.transform.localScale = new(1f, 1f);
            }
            if (inputPause)
            {
                yield return new WaitForSeconds(0.3f);
                inputPause = false;
            }
        }
        UnityEngine.Object.Destroy(leftArrow.transform.parent.gameObject);
        if (powerSlot >= 0)
        {
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
                        GrantCombatLevel();
                        break;
                    case "Spirit":
                        GrantSpiritLevel();
                        break;
                    case "Endurance":
                        GrantEnduranceLevel();
                        break;
                    default:
                        HeroController.instance.AddGeo(200);
                        break;
                }
            if (pickedPower != null)
            {
                CombatController.ObtainedPowers.Add(pickedPower);
                pickedPower.EnablePower();
                PowerSelected?.Invoke(pickedPower);
            }
            for (int i = 1; i < 4; i++)
                HistoryController.Archive.AddPowerData(shinyFsm.FsmVariables.FindFsmString("Option " + i).Value, powerSlot + 1 == i);
        }
        else if (powerSlot == -1)
        {
            PowerSelected?.Invoke(null);
            for (int i = 1; i < 4; i++)
                HistoryController.Archive.AddPowerData(shinyFsm.FsmVariables.FindFsmString("Option " + i).Value, false);
        }
        else
        {
            SecretController.LeftRolls--;
            amount = RollSelection(shinyFsm, (TreasureType)shinyFsm.FsmVariables.FindFsmInt("Item Select").Value);
            TrialOfCrusaders.Holder.StartCoroutine(SelectPower(CreatePowerOverlay(shinyFsm, amount), shinyFsm, amount));
            yield break;
        }
        shinyFsm.SendEvent("CHARM");
        shinyFsm.gameObject.GetComponent<LeftShinyFlag>().RemoveFlag();
        InventoryController.UpdateStats();
        InventoryController.UpdateList(-1);
        if (StageController.CurrentRoomNumber >= 1 && StageController.CurrentRoom.BossRoom && !StageController.QuietRoom)
            TrialOfCrusaders.Holder.StartCoroutine(StageController.WaitForTransition());
    }

    #endregion

    internal static void GrantCombatLevel()
    {
        if (CombatController.CombatCapped)
            return;
        CombatController.CombatLevel++;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    internal static void GrantSpiritLevel()
    {
        if (CombatController.SpiritCapped)
            return;
        CombatController.SpiritLevel++;
        PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
    }

    internal static void GrantEnduranceLevel()
    {
        if (CombatController.EnduranceCapped)
            return;
        CombatController.EnduranceLevel++;
        EnduranceHealthGrant = true;
        HeroController.instance.AddHealth(1);
        EnduranceHealthGrant = false;
        PlayMakerFSM.BroadcastEvent("MAX HP UP");
    }

    internal static T GetPower<T>() where T : Power => Powers.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
}
