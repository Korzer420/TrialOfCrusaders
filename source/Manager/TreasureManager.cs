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
using TrialOfCrusaders.Resources.Text;
using TrialOfCrusaders.UnityComponents.Other;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;
using Caching = TrialOfCrusaders.Powers.Common.Caching;

namespace TrialOfCrusaders.Manager;

public static class TreasureManager
{
    internal static string UncommonTextColor = "#33cc33";
    internal static string RareTextColor = "#00ffff";

    internal static MethodInfo InvulnerableCall = typeof(HeroController).GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance);

    private static Sprite _backgroundSprite;

    private static GameObject _glow;

    #region Initialize

    static TreasureManager() => PhaseManager.PhaseChanged += PhaseManager_PhaseChanged;

    private static void PhaseManager_PhaseChanged(Phase currentPhase, Phase newPhase)
    {
        if (newPhase == Phase.Run)
            SelectionCount = 0;
    }

    #endregion

    #region Properties

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
        // Shop update
        new Banish(),
        new Discount(),
        new RelicSeeker(),
        new SharedFood(),
        new Credit(),
        new BrittleShell(),
        new BrighterFuture(),
        new Gambling(),
        new Regrets(),
        // Special
        new VoidHeart(),
    ];

    public static int BadLuckProtection { get; set; } = 0;

    public static bool EnduranceHealthGrant { get; set; }

    public static bool SelectionActive { get; set; }

    public static int SelectionCount { get; set; }

    public static bool CanRerollHighroller => SecretRef.UnlockedHighRoller && SecretRef.LeftRolls > 0;

    public static bool CanRerollSeal => ConsumableRef.RerollSeals > 0;

    public static bool CanRerollGambling => PowerRef.HasPower(out Gambling gambling) && gambling.LeftRolls > 0;

    public static event Action<TreasureType, GameObject> SpawnedShiny;

    public static event Action<Power> PowerSelected;

    public static List<string> TreasurePool { get; set; } = [];

    #endregion

    internal static void SetupShiny(GameObject chest)
    {
        Shiny = GameObject.Instantiate(chest.transform.Find("Item").GetChild(0).gameObject);
        Shiny.name = "ToC Item";
        UnityEngine.Object.Destroy(Shiny.GetComponent<ObjectBounce>());
        UnityEngine.Object.Destroy(Shiny.GetComponent<PersistentBoolItem>());
        UnityEngine.Object.DontDestroyOnLoad(Shiny);

        BigItemUI = GameObject.Instantiate(Shiny.LocateMyFSM("Shiny Control").GetState("Dash").GetFirstAction<CreateUIMsgGetItem>().gameObject.Value);
        BigItemUI.SetActive(false);
        GameObject.DontDestroyOnLoad(BigItemUI);
    }

    internal static void PrepareTreasureRoom(RoomData currentRoom)
    {
        UnityEngine.Object.Destroy(GameObject.Find("Godseeker EngineRoom NPC"));
        GameObject pedestal = new("Pedestal");
        pedestal.AddComponent<SpriteRenderer>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Other.Pedestal");
        pedestal.transform.position = new(94.23f, 14.8f, -0.1f);
        pedestal.AddComponent<BoxCollider2D>().size = new(2f, 2.5f);
        pedestal.layer = 8; // Terrain layer
        pedestal.SetActive(true);
        // If we are in a quiet room even though the room flag isn't set, we are in a treasure or shop room instead.
        if (!currentRoom.IsQuietRoom)
            TreasureManager.SpawnShiny(RngManager.GetRandom(0, 100) < 10 ? TreasureType.RareOrb : TreasureType.NormalOrb, new(94.23f, 16.4f), false);
        else
        {
            if (currentRoom.Name == "Quake" || currentRoom.Name == "Fireball")
            {
                TreasureType intendedSpell = (TreasureType)Enum.Parse(typeof(TreasureType), currentRoom.Name);
                if (intendedSpell == TreasureType.Fireball && PDHelper.FireballLevel != 0
                    || intendedSpell == TreasureType.Quake && PDHelper.QuakeLevel != 0)
                {
                    TreasureManager.SpawnShiny(TreasureType.RareOrb, new(94.23f, 16.4f), false);
                    return;
                }
            }
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GG_Engine")
                TreasureManager.SpawnShiny((TreasureType)Enum.Parse(typeof(TreasureType), currentRoom.Name), new(94.23f, 16.4f), false);
        }
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
            if (PowerRef.HasPower(out Gambling gambling))
                gambling.LeftRolls = 2;
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
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Claw:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Claw_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_WALLJUMP";
                    PDHelper.HasWalljump = true;
                    PDHelper.CanWallJump = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.CrystalHeart:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Crystal_Dash_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SUPERDASH";
                    PDHelper.HasSuperDash = true;
                    PDHelper.CanSuperDash = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Wings:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Wings_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_DOUBLEJUMP";
                    PDHelper.HasDoubleJump = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Tear:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Tear_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_ACIDARMOUR";
                    PDHelper.HasAcidArmour = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.ShadeCloak:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Shade_Dash_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SHADOWDASH";
                    PDHelper.HasShadowDash = true;
                    PDHelper.CanShadowDash = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Lantern:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Lantern_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_LANTERN";
                    PDHelper.HasLantern = true;
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Fireball:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Vengeful_Spirit_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPELL_FIREBALL1";
                    Power fireball = Powers.First(x => x.GetType() == typeof(VengefulSpirit));
                    PowerRef.ObtainedPowers.Add(fireball);
                    fireball.EnablePower();
                    StageRef.EnableExit();
                    fsm.SendEvent("TRINKET");
                    break;
                case TreasureType.Quake:
                    fsm.GetState("Trink 1").GetFirstAction<SetSpriteRendererSprite>().sprite = SpriteHelper.CreateSprite<TrialOfCrusaders>("Sprites.Icons.Desolate_Dive_Icon");
                    fsm.GetState("Trink 1").GetFirstAction<GetLanguageString>().convName.Value = "INV_NAME_SPELL_QUAKE1";
                    Power quake = Powers.First(x => x.GetType() == typeof(DesolateDive));
                    PowerRef.ObtainedPowers.Add(quake);
                    quake.EnablePower();
                    StageRef.EnableExit();
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
            if (!StageRef.QuietRoom)
                HeroController.instance.StartCoroutine((IEnumerator)InvulnerableCall.Invoke(HeroController.instance, [2f]));
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
                    SecretRef.SetupItemScreen(fsm, treasure);
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

    private static int RollSelection(PlayMakerFSM fsm, TreasureType treasureType, bool gambling = false)
    {
        if (treasureType == TreasureType.NormalOrb || treasureType == TreasureType.RareOrb)
        {
            bool rare = false;
            List<Power> selectedPowers = [];
            List<string> statBoni = [];
            List<Power> availablePowers = [];
            List<string> removeFromTreasurePool = [];
            foreach (string powerName in TreasurePool)
            {
                Power selectedPower = null;
                foreach (Power power in Powers)
                    if (power.Name == powerName)
                    {
                        selectedPower = power;
                        break;
                    }
                if (selectedPower?.CanAppear == true)
                {
                    if (!PowerRef.ObtainedPowers.Contains(selectedPower) || powerName == "Cocoon"
                        || powerName == "Regret")
                        availablePowers.Add(selectedPower);
                    else
                        removeFromTreasurePool.Add(powerName);
                }
                else
                    removeFromTreasurePool.Add(powerName);
            }
            // Remove unobtainable powers from the pool.
            if (removeFromTreasurePool.Count > 0)
                TreasurePool.RemoveAll(x => removeFromTreasurePool.Contains(x));

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
                    //    selectedPowers.Add(Powers.First(x => x.GetType() == typeof(Credit)));
                    //else if (i == 1)
                    //    selectedPowers.Add(Powers.First(x => x.GetType() == typeof(Weaversong)));
                    //else
                    selectedPowers.Add(powerPool[RngManager.GetRandom(0, powerPool.Count - 1)]);
                }
                availablePowers.Remove(selectedPowers.Last());
                Power selectedPower = selectedPowers.Last();
                (float, float, float) bonusChances = selectedPower.BonusRates;
                int rolledBonus = RngManager.GetRandom(1, 100);
                if (selectedPower.Tier != Rarity.Rare && PowerRef.HasPower<LuckyCharm>(out _))
                    bonusChances = new(bonusChances.Item1 * 1.5f, bonusChances.Item2 * 1.5f, bonusChances.Item3 * 2);
                if (rolledBonus <= bonusChances.Item1 && !CombatRef.CombatCapped)
                    statBoni.Add("Combat");
                else if (rolledBonus <= bonusChances.Item1 + bonusChances.Item2 && !CombatRef.SpiritCapped)
                    statBoni.Add("Spirit");
                else if (rolledBonus <= bonusChances.Item1 + bonusChances.Item2 + bonusChances.Item3 && !CombatRef.EnduranceCapped)
                    statBoni.Add("Endurance");
                else
                    statBoni.Add(null);
            }
            if (selectedPowers.Count != 0)
            {
                if (!rare)
                    BadLuckProtection = Math.Min(BadLuckProtection + 2, 64);
                for (int i = 0; i < selectedPowers.Count; i++)
                {
                    string selectionValue = string.IsNullOrEmpty(statBoni[i])
                                ? selectedPowers[i].Name
                                : $"{selectedPowers[i].Name}_{statBoni[i]}";
                    if (!gambling || RngManager.GetRandom(0, 5) == 0)
                        fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value = selectionValue;
                    else
                        fsm.FsmVariables.FindFsmString("Option " + (i + 1)).Value = "Geo";
                }
                return selectedPowers.Count;
            }
        }

        int optionAmount = 3;
        List<string> options = [];
        if (!CombatRef.CombatCapped)
            options.Add("Combat");
        if (!CombatRef.SpiritCapped)
            options.Add("Spirit");
        if (!CombatRef.EnduranceCapped)
            options.Add("Endurance");
        if (treasureType == TreasureType.CatchUpStat)
        {
            int[] amounts = [CombatRef.CombatLevel, CombatRef.SpiritLevel, CombatRef.EnduranceLevel];
            int max = amounts.Max();
            if (amounts.Count(x => x == max) == 1)
            {
                optionAmount = 2;
                if (CombatRef.CombatLevel == max)
                    options.Remove("Combat");
                else if (CombatRef.SpiritLevel == max)
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
        text.text = $"<color={CombatController.CombatStatColor}>Combat: {CombatRef.CombatLevel}</color>";
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
        text.text = $"<color={CombatController.SpiritStatColor}>Spirit: {CombatRef.SpiritLevel}</color>";
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
        text.text = $"<color={CombatController.EnduranceStatColor}>Endurance: {CombatRef.EnduranceLevel}</color>";
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
        description.textContainer.size = new(3f, 5f);
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
                "Combat" => ShopText.Stat_Combat_Title,
                "Spirit" => ShopText.Stat_Spirit_Title,
                "Endurance" => ShopText.Stat_Endurance_Title,
                _ => "200 Geo"
            };
            description.text = powerName switch
            {
                "Combat" => ShopText.Stat_Combat_Desc,
                "Spirit" => ShopText.Stat_Spirit_Desc,
                "Endurance" => ShopText.Stat_Endurance_Desc,
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
        bool highroller = CanRerollHighroller;
        bool seals = CanRerollSeal;
        bool gambling = PowerRef.HasPower(out Gambling gamblingPower) && gamblingPower.LeftRolls > 0;
        if (!highroller && !seals && !gambling)
            return;
        string[] texts =
        [
            highroller ? $"Reroll (Highroller; {SecretRef.LeftRolls} left)" : null,
            seals ? $"Reroll (Seals; {ConsumableRef.RerollSeals} left)" : null,
            gambling ? $"Reroll (Gambling; {gamblingPower.LeftRolls} left)" : null
        ];
        for (int i = 0; i < 3; i++)
        {
            if (texts[i] == null)
                continue;
            (SpriteRenderer, TextMeshPro) optionPair = TextManager.CreateUIObject("Reroll power");
            GameObject option = optionPair.Item2.gameObject;
            option.transform.SetParent(parent);
            GameObject.Destroy(optionPair.Item1);
            optionPair.Item2.text = texts[i];
            option.transform.position = new(-13.9f, -5f + i);
            optionPair.Item2.fontSize = 3;
        }
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
        bool canReroll = shinyFsm.FsmVariables.FindFsmInt("Item Select").Value < 2;
        while (!selected)
        {
            yield return null;
            if (InputHandler.Instance.inputActions.jump.IsPressed || InputHandler.Instance.inputActions.menuSubmit.IsPressed)
                selected = true;
            else if (InputHandler.Instance.inputActions.left.IsPressed)
            {
                powerSlot--;
                if (powerSlot <= -5)
                    powerSlot = amount - 1;
                else if (powerSlot <= -2)
                {
                    for (; powerSlot > -5; powerSlot--)
                    {
                        if (powerSlot == -2 && CanRerollHighroller)
                            break;
                        else if (powerSlot == -3 && CanRerollSeal)
                            break;
                        else if (powerSlot == -4 && CanRerollGambling)
                            break;
                    }
                    if (powerSlot == -5)
                        powerSlot = amount - 1;
                }
                inputPause = true;
            }
            else if (InputHandler.Instance.inputActions.right.IsPressed)
            {
                powerSlot++;
                if (powerSlot >= amount)
                {
                    powerSlot = -1;
                    if (canReroll)
                    {
                        if (CanRerollGambling)
                            powerSlot = -4;
                        else if (CanRerollSeal)
                            powerSlot = -3;
                        else if (CanRerollHighroller)
                            powerSlot = -2;
                    }
                }

                inputPause = true;
            }
            if (powerSlot >= 0)
            {
                leftArrow.transform.localPosition = new(-9f + powerSlot * 8.5f, 1.7f);
                rightArrow.transform.localPosition = new(-3.4f + powerSlot * 8.5f, 1.7f);
                leftArrow.transform.localScale = new(3f, 3f);
                rightArrow.transform.localScale = new(3f, 3f);

                if (!SecretRef.UnlockedHighRoller)
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
                leftArrow.transform.localPosition = new(-15f, -6.2f + (powerSlot * -1));
                rightArrow.transform.localPosition = new(-9.25f, -6.2f + (powerSlot * -1));
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
                PowerRef.ObtainedPowers.Add(pickedPower);
                bool pickedRegrets = pickedPower.GetType() == typeof(Regrets);
                if (pickedPower.GetType() != typeof(Cocoon) && !pickedRegrets)
                    TreasurePool.RemoveAll(x => x == pickedPower.Name);

                bool regretsOffered = false;
                for (int i = 1; i < 3; i++)
                {
                    string powerName = shinyFsm.FsmVariables.FindFsmString("Option " + (powerSlot + 1))?.Value;
                    if (!string.IsNullOrEmpty(powerName) && powerName == "Regrets")
                    { 
                        regretsOffered = true;
                        break;
                    }
                }
                if (!pickedRegrets && regretsOffered)
                    TreasurePool.AddRange(["Regrets", "Regrets"]);

                pickedPower.EnablePower();
                PowerSelected?.Invoke(pickedPower);
            }
            for (int i = 1; i < 4; i++)
                HistoryRef.Archive.AddPowerData(shinyFsm.FsmVariables.FindFsmString("Option " + i).Value, powerSlot + 1 == i);
        }
        else if (powerSlot == -1)
        {
            PowerSelected?.Invoke(null);
            for (int i = 1; i < 4; i++)
                HistoryRef.Archive.AddPowerData(shinyFsm.FsmVariables.FindFsmString("Option " + i).Value, false);
            if (PowerRef.HasPower<BrighterFuture>(out _))
                HeroController.instance.AddGeo(200);
        }
        else
        {
            if (powerSlot == -2)
                SecretRef.LeftRolls--;
            else if (powerSlot == -3)
                ConsumableRef.RerollSeals--;
            else
                (PowerRef.ObtainedPowers.First(x => x.GetType() == typeof(Gambling)) as Gambling).LeftRolls--;
            amount = RollSelection(shinyFsm, (TreasureType)shinyFsm.FsmVariables.FindFsmInt("Item Select").Value, powerSlot == -4);
            TrialOfCrusaders.Holder.StartCoroutine(SelectPower(CreatePowerOverlay(shinyFsm, amount), shinyFsm, amount));
            yield break;
        }
        shinyFsm.SendEvent("CHARM");
        shinyFsm.gameObject.GetComponent<LeftShinyFlag>().RemoveFlag();
        InventoryController.UpdateStats();
        InventoryController.UpdateList(-1);
        if (StageRef.CurrentRoomNumber >= 1 && StageRef.CurrentRoom.BossRoom && !StageRef.QuietRoom)
            TrialOfCrusaders.Holder.StartCoroutine(StageRef.InitiateTransition());
    }

    #endregion

    internal static void GrantCombatLevel()
    {
        if (CombatRef.CombatCapped)
            return;
        CombatRef.CombatLevel++;
        PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMAGE");
    }

    internal static void GrantSpiritLevel()
    {
        if (CombatRef.SpiritCapped)
            return;
        CombatRef.SpiritLevel++;
        PlayMakerFSM.BroadcastEvent("NEW SOUL ORB");
    }

    internal static void GrantEnduranceLevel()
    {
        if (CombatRef.EnduranceCapped)
            return;
        CombatRef.EnduranceLevel++;
        EnduranceHealthGrant = true;
        HeroController.instance.AddHealth(1);
        EnduranceHealthGrant = false;
        PlayMakerFSM.BroadcastEvent("MAX HP UP");
    }

    internal static T GetPower<T>() where T : Power => Powers.FirstOrDefault(x => x.GetType() == typeof(T)) as T;
}
