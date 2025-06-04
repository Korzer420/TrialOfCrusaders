using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents.CombatElements;
using TrialOfCrusaders.UnityComponents.Debuffs;
using TrialOfCrusaders.UnityComponents.PowerElements;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

/// <summary>
/// Handles everything related to the combat and powers.
/// </summary>
internal static class CombatController
{
    private static bool _enabled;
    private static ILHook _attackMethod;
    private static Coroutine _enemyScanner;
    public const int InstaKillDamage = 500;
    private static int _stageTreasures = 0;

    #region Properties

    public static int CombatLevel { get; set; }

    public static int SpiritLevel { get; set; }

    public static int EnduranceLevel { get; set; }

    public static bool CharmUpdate { get; set; }

    public static List<HealthManager> ActiveEnemies { get; set; } = [];

    public static List<Power> ObtainedPowers { get; set; } = [];

    public static bool InCombat { get; set; }

    // As this is called very frequently we store it in an extra value.
    internal static bool DebuffsStronger { get; set; }

    #endregion

    #region Events

    public static event Action TookDamage;

    public static event Action EnemiesCleared;

    public static event Action<HealthManager> EnemyKilled;

    public static event Action BeginCombat;

    #endregion

    #region Power Utils
   
    public static bool HasPower<T>(out T selectedPower) where T : Power
    {
        foreach (Power power in ObtainedPowers)
            if (power.GetType() == typeof(T))
            {
                selectedPower = (T)power;
                return true;
            }
        selectedPower = null;
        return false;
    }

    public static bool HasNailArt() => PDHelper.HasDashSlash || PDHelper.HasUpwardSlash || PDHelper.HasCyclone;

    public static bool HasSpell() => PDHelper.FireballLevel + PDHelper.QuakeLevel + PDHelper.ScreamLevel > 0;

    internal static void DisablePowers()
    {
        foreach (Power power in ObtainedPowers)
            power.DisablePower();
    } 

    #endregion

    #region Setup

    public static void Initialize()
    {
        if (_enabled)
            return;
        LogManager.Log("Enable Combat Controller");

        // Setup hooks.
        On.PlayMakerFSM.OnEnable += FsmEdits;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        On.HealthManager.TakeDamage += ModifyDealtDamage;
        On.HealthManager.OnEnable += ModifyEnemy;
        On.HealthManager.Die += OnEnemyDeath;
        IL.HeroController.Move += ModifyMovementSpeed;
        IL.HeroController.Attack += ModifyAttackDuration;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += ModifyOtherDealtDamage;
        ModHooks.SoulGainHook += ModifySoulGain;
        On.HeroController.TakeDamage += ModifyTakenDamage;
        On.HeroController.AddGeo += ModifyGeoAdd;
        IL.HeroController.MaxHealth += BlockCharmHeal;
        On.HeroController.CharmUpdate += FlagCharmHeal;
        On.HeroController.FinishedEnteringScene += FinalizeEnemies;
        On.HeroController.Die += OnPlayerDeath;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter += CheckForFailedRun;
        ModHooks.OnEnableEnemyHook += ForceEnemiesActive;
        HistoryController.CreateEntry += PassHistoryData;
        // This is called upon leaving a godhome room and would restore the health + remove lifeblood.
        IL.BossSequenceController.RestoreBindings += BlockHealthReset;
        On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.OnEnter += AdjustLifebloodPosition;
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += MoveLifebloodInFront;
        On.BossSceneController.Start += TrackBosses;
        _attackMethod = new(typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance), ModifyAttackSpeed);
        StageController.RoomEnded += StageController_RoomEnded;

        CreateExtraHealth();
        _enemyScanner = TrialOfCrusaders.Holder.StartCoroutine(ScanEnemies());
        GameCameras.instance.hudCanvas.gameObject.SetActive(false);
        GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        CoroutineHelper.WaitUntil(() =>
        {
            // Adjust max health and max mp.
            int intendedMaxHealth = 5 + EnduranceLevel;
            if (PDHelper.MaxHealth != intendedMaxHealth)
            {
                PDHelper.MaxHealth += intendedMaxHealth - PDHelper.MaxHealth;
                HeroController.instance.MaxHealth();
            }

            int maxMP = 33 + Math.Min(SpiritLevel, 18) * 8 + (SpiritLevel == 19 ? 10 : 0);
            int soulVessel = 0;
            if (maxMP >= 165)
                soulVessel = 3;
            else if (maxMP > 132)
                soulVessel = 2;
            else if (maxMP > 99)
                soulVessel = 1;
            PDHelper.MPReserveMax = soulVessel * 33;
        }, () => HeroController.instance?.acceptingInput == true, true);
        _enabled = true;
    }

    public static void Unload()
    {
        if (!_enabled)
            return;
        LogManager.Log("Disable Combat Controller");
        On.PlayMakerFSM.OnEnable -= FsmEdits;
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        On.HealthManager.TakeDamage -= ModifyDealtDamage;
        On.HealthManager.OnEnable -= ModifyEnemy;
        On.HealthManager.Die -= OnEnemyDeath;
        IL.HeroController.Move -= ModifyMovementSpeed;
        IL.HeroController.Attack -= ModifyAttackDuration;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= ModifyOtherDealtDamage;
        ModHooks.SoulGainHook -= ModifySoulGain;
        On.HeroController.TakeDamage -= ModifyTakenDamage;
        On.HeroController.AddGeo -= ModifyGeoAdd;
        IL.HeroController.MaxHealth -= BlockCharmHeal;
        On.HeroController.CharmUpdate -= FlagCharmHeal;
        On.HeroController.FinishedEnteringScene -= FinalizeEnemies;
        On.HeroController.Die -= OnPlayerDeath;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter -= CheckForFailedRun;
        ModHooks.OnEnableEnemyHook -= ForceEnemiesActive;
        HistoryController.CreateEntry -= PassHistoryData;
        IL.BossSequenceController.RestoreBindings -= BlockHealthReset;
        On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.OnEnter -= AdjustLifebloodPosition;
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter -= MoveLifebloodInFront;
        On.BossSceneController.Start -= TrackBosses;

        _attackMethod?.Dispose();
        StageController.RoomEnded -= StageController_RoomEnded;

        if (_enemyScanner != null)
            TrialOfCrusaders.Holder.StopCoroutine(_enemyScanner);
        DisablePowers();
        ObtainedPowers.Clear();
        _enabled = false;
    }

    #endregion

    #region Health Setup

    private static void CreateExtraHealth()
    {
        Transform cameraObject = GameObject.Find("_GameCameras").transform;
        if (cameraObject.Find("HudCamera/Hud Canvas/Health/Health 12") != null)
            return;
        GameObject healthPrefab = cameraObject.Find("HudCamera/Hud Canvas/Health/Health 11").gameObject;
        float space = healthPrefab.transform.localPosition.x - cameraObject.Find("HudCamera/Hud Canvas/Health/Health 10").localPosition.x;

        for (int i = 1; i <= 14; i++)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(healthPrefab, cameraObject.Find("HudCamera/Hud Canvas/Health"));
            gameObject.name = "Health " + (i + 11);
            gameObject.LocateMyFSM("health_display").FsmVariables.FindFsmInt("Health Number").Value = i + 11;
            gameObject.transform.localPosition = new Vector3(healthPrefab.transform.localPosition.x + space * i, healthPrefab.transform.localPosition.y, healthPrefab.transform.localPosition.z);
        }
    }

    private static void FlagCharmHeal(On.HeroController.orig_CharmUpdate orig, HeroController self)
    {
        // Flag the charm update so the heal can be blocked.
        CharmUpdate = true;
        orig(self);
        CharmUpdate = false;
    }

    private static void BlockCharmHeal(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);

        ILLabel label;
        cursor.GotoNext(x => x.MatchRet());
        label = cursor.MarkLabel();
        cursor.Goto(0);
        cursor.EmitDelegate(() => CharmUpdate);
        cursor.Emit(OpCodes.Brtrue, label);
    }

    private static IEnumerator UpdateUI()
    {
        yield return new WaitForSeconds(.2f);
        try
        {
            HeroController.instance.proxyFSM.SendEvent("HeroCtrl-Healed");
        }
        catch (Exception exception)
        {
            LogManager.Log("Failed to send hero heal", exception);
        }
    }

    private static void CheckForFailedRun(On.HutongGames.PlayMaker.Actions.IntSwitch.orig_OnEnter orig, IntSwitch self)
    {
        if (self.IsCorrectContext("Hero Death Anim", "Hero Death", "Check MP"))
        {
            self.Fsm.Variables.FindFsmBool("Soul Cracked").Value = false;
            PhaseController.TransitionTo(Phase.Lobby);
        }
        orig(self);
    }

    private static void BlockHealthReset(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Emit(OpCodes.Ret);
    }

    private static void MoveLifebloodInFront(On.HutongGames.PlayMaker.Actions.SetPosition.orig_OnEnter orig, SetPosition self)
    {
        if (self.IsCorrectContext("blue_health_display", null, "Init"))
            self.z.Value -= 0.1f;
        orig(self);
    }

    private static void AdjustLifebloodPosition(On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.orig_OnEnter orig, ConvertIntToFloat self)
    {
        orig(self);
        if (self.IsCorrectContext("Blue Health Control", "Health", "Add Blue Health"))
            self.floatVariable.Value = PDHelper.HealthBlue;
    }

    #endregion

    #region Enemy Control

    private static void ModifyEnemy(On.HealthManager.orig_OnEnable orig, HealthManager self)
    {
        try
        {
            // Prevent "immortal" enemies and some that cause issues.
            if (self.hp != 9999 && self.gameObject.name != "Mender Bug" && !self.gameObject.name.Contains("Pigeon") && !self.gameObject.name.Contains("Hatcher Baby Spawner")
                && self.gameObject.name != "Hollow Shade(Clone)" && !self.gameObject.name.Contains("fluke_baby")
                && self.gameObject.name != "Cap Hit" && !self.gameObject.name.Contains("Baby Centipede Spawner")
                && !self.gameObject.name.Contains("Zombie Spider") && !self.gameObject.name.Contains("Hiveling Spawner"))
            {
                if (self.hp >= 190f && StageController.CurrentRoom.BossRoom)
                    self.gameObject.AddComponent<BossFlag>();
                if (!InCombat)
                    ActiveEnemies.Add(self);
                if (self.hp != 1)
                {
                    if (StageController.CurrentRoomNumber >= 20)
                    {
                        float scaling = 0.1f;
                        // Pure Vessel and NKG receive a greater scaling than other bosses as an attempt to match the difficulty with Radiance.
                        if ((StageController.CurrentRoomNumber != StageController.CurrentRoomData.Count || StageController.CurrentRoomData[StageController.CurrentRoomIndex].Name == "GG_Radiance")
                            && StageController.CurrentRoomData[StageController.CurrentRoomIndex].BossRoom)
                            scaling = 0.05f;
                        self.hp = Mathf.CeilToInt(self.hp * (1 + (StageController.CurrentRoomNumber - 20) * scaling));
                    }
                    else if (StageController.CurrentRoomNumber < 10)
                        self.hp /= 2;
                }
            }
        }
        catch (Exception exception)
        {
            LogManager.Log("Failed to modify enemy.", exception);
        }
        orig(self);
    }

    private static void OnEnemyDeath(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        bool contained = false;
        try
        {
            contained = ActiveEnemies.Contains(self);
            ActiveEnemies.Remove(self);
            UpdateEnemies();
            if (contained && self.GetComponent<BaseEnemy>() is BaseEnemy enemyFlag)
            {
                EnemyKilled?.Invoke(self);
                if (HasPower<RoyalDecree>(out _))
                {
                    LogManager.Log("Check royal decree target");
                    if (self.GetComponent<RoyalMark>() is RoyalMark mark)
                    {
                        LogManager.Log("Killed enemy had mark.");
                        if (ActiveEnemies.Count == 0)
                        {
                            LogManager.Log("No enemies left. Grant reward.");
                            int rolled = RngManager.GetStageRandom(1, 100);
                            if (rolled <= 2)
                                TreasureManager.SpawnShiny(TreasureType.RareOrb, self.transform.position);
                            else if (rolled <= 10)
                                TreasureManager.SpawnShiny(TreasureType.NormalOrb, self.transform.position);
                            else if (rolled <= 35)
                                TreasureManager.SpawnShiny(TreasureType.PrismaticOrb, self.transform.position);
                            else
                                HeroController.instance.AddGeo(100);
                        }
                        else
                        {
                            LogManager.Log("Find new enemy target");
                            mark.CorrectPosition(ActiveEnemies[RngManager.GetRandom(0, ActiveEnemies.Count - 1)]);
                            LogManager.Log("New enemy is: " + mark.AttachedEnemy.name);
                        }
                    }
                    else
                    {
                        HealthManager enemy = ActiveEnemies.FirstOrDefault(x => x.GetComponent<RoyalMark>());
                        if (enemy != null)
                            Component.Destroy(enemy.GetComponent<RoyalMark>());
                    }
                }
                if (!enemyFlag.NoLoot && !StageController.CurrentRoom.BossRoom)
                {
                    float rolled = RngManager.GetStageRandom(0f, 100f);
                    if (rolled <= 4f / (1 + _stageTreasures))
                    {
                        TreasureManager.SpawnShiny(TreasureType.NormalOrb, self.transform.position);
                        _stageTreasures++;
                    }
                    else if (rolled <= 12f / (1 + _stageTreasures))
                    {
                        _stageTreasures++;
                        List<int> amounts = [CombatLevel, SpiritLevel, EnduranceLevel];
                        int max = amounts.Max();
                        int maxStats = amounts.Count(x => x == max);
                        if (maxStats == 3)
                            TreasureManager.SpawnShiny(TreasureType.PrismaticOrb, self.transform.position);
                        else if (maxStats == 2)
                        {
                            int nonMaxedStat = amounts.FindIndex(x => x != max);
                            if (amounts[nonMaxedStat] + 4 < max)
                                TreasureManager.SpawnShiny((TreasureType)(nonMaxedStat + 3), self.transform.position);
                            else
                                TreasureManager.SpawnShiny(TreasureType.PrismaticOrb, self.transform.position);
                        }
                        else
                        {
                            // If stats are greatly unbalanced, we force an off main stat.
                            int maxStatIndex = amounts.FindIndex(x => x == max);
                            bool bigStatGap = maxStatIndex switch
                            {
                                0 => max > SpiritLevel + EnduranceLevel + 2,
                                1 => max > CombatLevel + EnduranceLevel + 2,
                                _ => max > CombatLevel + SpiritLevel + 2,
                            };
                            if (bigStatGap)
                                TreasureManager.SpawnShiny(TreasureType.CatchUpStat, self.transform.position);
                            else
                                TreasureManager.SpawnShiny(TreasureType.PrismaticOrb, self.transform.position);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Error in HealthManager Die", ex);
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
        try
        {
            if (ActiveEnemies.Count == 0 && !StageController.QuietRoom && contained 
                && !StageController.CurrentRoom.BossRoom)
                FireEnemiesCleared();
        }
        catch (Exception ex)
        {
            LogManager.Log("Error in HealthManager Die", ex);
        }
    }

    private static void FinalizeEnemies(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        try
        {
            if (InCombat)
                return;
            ActiveEnemies ??= [];
            List<HealthManager> newEnemies = [];
            foreach (HealthManager item in ActiveEnemies)
                if (item != null && item.gameObject != null && item.gameObject.scene != null && item.gameObject.scene.name == GameManager.instance.sceneName)
                {
                    if (StageController.CurrentRoom.BossRoom && item.GetComponent<BossFlag>())
                        item.OnDeath += Boss_OnDeath;
                    item.gameObject.AddComponent<BaseEnemy>();
                    newEnemies.Add(item);
                }
            ActiveEnemies = newEnemies;
            if (!StageController.QuietRoom)
            {
                InCombat = true;
                LogManager.Log("Required enemy amount: " + ActiveEnemies.Count);
                if (StageController.CurrentRoom.BossRoom)
                    _bossCounter = GameManager.instance.sceneName switch
                    {
                        "GG_Watcher_Knights" => 6,
                        "GG_Soul_Master" or "GG_Oblobbles" or "GG_Soul_Tyrant" or "GG_Vengefly_V" => 2,
                        _ => 1
                    };
                else if (HasPower<RoyalDecree>(out _) && ActiveEnemies.Count > 0)
                {
                    GameObject royalMark = new("Royal Decree");
                    royalMark.SetActive(false);
                    royalMark.AddComponent<RoyalMark>().CorrectPosition(ActiveEnemies[UnityEngine.Random.Range(0, ActiveEnemies.Count)]);
                    royalMark.SetActive(true);
                }
                BeginCombat.Invoke();
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to finalize enemies", ex);
        }
    }

    private static bool ForceEnemiesActive(GameObject enemy, bool isAlreadyDead) => false;

    private static IEnumerator ScanEnemies()
    {
        // I really, really, really, really, really, really, really, really, really, really, really wanted to avoid doing shit like this
        // But since the enemy system in this game is so fucking scuffed, we unfortunately have to do this to prevent issues.
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (StageController.QuietRoom || !InCombat)
                continue;
            UpdateEnemies();
        }
    }

    private static void UpdateEnemies()
    {
        try
        {
            int currentCount = ActiveEnemies.Count;
            // Unity doesn't like the "?" operator.
            for (int i = 0; i < ActiveEnemies.Count; i++)
                if (ActiveEnemies[i] == null || ActiveEnemies[i].gameObject == null || !ActiveEnemies[i].gameObject.activeSelf)
                {
                    ActiveEnemies.RemoveAt(i);
                    i--;
                }
            if (ActiveEnemies.Count == 0 && !StageController.CurrentRoom.BossRoom)
                FireEnemiesCleared();
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to update enemies", ex);
        }
    }

    private static int _bossCounter = 0;

    private static IEnumerator TrackBosses(On.BossSceneController.orig_Start orig, BossSceneController self)
    {
        try
        {
            foreach (HealthManager boss in self.bosses)
                if (!ActiveEnemies.Contains(boss))
                {
                    boss.gameObject.AddComponent<BaseEnemy>();
                    boss.gameObject.AddComponent<BossFlag>();
                    ActiveEnemies.Add(boss);
                    boss.OnDeath += Boss_OnDeath;
                }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to track bosses", ex);
        }
        yield return orig(self);
    }

    private static void Boss_OnDeath()
    {
        try
        {
            _bossCounter--;
            if (_bossCounter == 0)
            {
                InCombat = false;
                if (StageController.CurrentRoomIndex == StageController.CurrentRoomData.Count - 1)
                {
                    if (GameManager.instance.sceneName == "GG_Hollow_Knight")
                        TrialOfCrusaders.Holder.StartCoroutine(StageController.WaitForTransition());
                }
                else
                {
                    // Place shiny at godseeker location.
                    // This should ensure the shiny lands on a platform. (Except for No Eyes)
                    GameObject crowd = GameObject.Find("Godseeker Crowd");
                    TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(crowd.transform.position.x + (GameManager.instance.sceneName == "GG_Flukemarm"
                        ? -7f
                        : GameManager.instance.sceneName == "GG_Ghost_No_Eyes_V" ? -5f : 0f)
                        , crowd.transform.position.y - 1f, HeroController.instance.transform.position.z), false);
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to count boss death", ex);
        }
    }
    
    internal static void FireEnemiesCleared()
    {
        if (!InCombat)
            return;
        LogManager.Log("All required enemies killed.");
        InCombat = false;
        EnemiesCleared.Invoke();
    }


    #endregion

    #region Global Event Handler

    private static int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.instance.nailDamage))
            orig = 3 + CombatLevel * 2 + (CombatLevel >= 19 ? 1 : 0);
        else if (name == nameof(PlayerData.maxHealthCap))
            return 25;
        else if (name == nameof(PlayerData.maxHealthBase) || name == nameof(PlayerData.maxHealth))
            return 5 + EnduranceLevel;
        else if (name == nameof(PlayerData.maxMP))
            return Math.Min(99, 33 + Math.Min(SpiritLevel, 18) * 8 + (SpiritLevel == 19 ? 10 : 0));
        else if (name == nameof(PlayerData.MPReserveMax))
        {
            if (SpiritLevel < 9)
                return 0;
            else if (SpiritLevel == 20)
                return 99;
            else if (SpiritLevel == 19)
                return 88;
            int spirit = Math.Max(0, -2 + (SpiritLevel - 8) * 8);
            return spirit;
        }
        return orig;
    }

    private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
    {
        if (name == nameof(PlayerData.overcharmed))
            return HasPower<VoidHeart>(out _);
        else if (name == nameof(PlayerData.isInvincible))
            return orig || TreasureManager.SelectionActive;
        return orig;
    }

    private static void ModifyDealtDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        try
        {
            if (hitInstance.AttackType == AttackTypes.Nail)
            {
                if (HasPower(out FragileStrength strength) && strength.StrengthActive)
                    hitInstance.DamageDealt *= 3;

                if (HasPower<Initiative>(out _) && self.GetComponent<InitiativeEffect>() == null)
                {
                    self.gameObject.AddComponent<InitiativeEffect>();
                    HeroController.instance.AddMPCharge(Math.Max(2, SpiritLevel / 2));
                    hitInstance.DamageDealt += 10 + CombatLevel * 2;
                }

                if (HasPower(out MantisStyle mantisStyle) && mantisStyle.Parried)
                {
                    mantisStyle.Parried = false;
                    hitInstance.DamageDealt += 40 + CombatLevel;
                }

                // Nail art: Hit L, Hit R, Great Slash, Dash Slash
                if (HasPower(out Acrobat acrobat) && acrobat.Buff)
                    hitInstance.DamageDealt += 2 + Mathf.FloorToInt(CombatLevel * 1.5f);

                // Armor scaling
                int armor = StageController.CurrentRoomNumber / 8;
                if (HasPower<BrutalStrikes>(out _))
                    if (hitInstance.Source?.name == "Great Slash" || hitInstance.Source?.name == "Dash Slash"
                        || hitInstance.Source?.name == "Hit L" || hitInstance.Source?.name == "Hit R")
                        armor = 0;
                if (armor > 0 && HasPower(out Shatter shatter))
                {
                    if (shatter.LastEnemy == self)
                        shatter.Stacks = Math.Min(10, shatter.Stacks + 1);
                    else
                    {
                        shatter.Stacks = 0;
                        shatter.LastEnemy = self;
                    }
                    armor = Math.Max(0, armor - shatter.Stacks);
                }
                hitInstance.DamageDealt = Math.Max(1, hitInstance.DamageDealt - armor);

                if (self.GetComponent<ConcussionEffect>() != null)
                {
                    hitInstance.DamageDealt = Mathf.FloorToInt(hitInstance.DamageDealt * 1.25f);
                    hitInstance.MagnitudeMultiplier *= 1.2f;
                }
            }
            else
            {
                if (HasPower<Initiative>(out _) && self.GetComponent<InitiativeEffect>() == null)
                {
                    self.gameObject.AddComponent<InitiativeEffect>();
                    HeroController.instance.AddMPCharge(Math.Max(2, SpiritLevel / 2));
                    hitInstance.DamageDealt += 10 + CombatLevel * 2;
                }
            }

            if (self.GetComponent<ShatteredMindEffect>() is ShatteredMindEffect effect)
                hitInstance.DamageDealt += effect.ExtraDamage;
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify dealt damage.", ex);
        }

#if DEBUG
        //hitInstance.DamageDealt = Math.Max(50, hitInstance.DamageDealt);
#endif
        orig(self, hitInstance);
    }

    private static void ModifyMovementSpeed(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH_COMBO)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 2f;
            if (HasPower<EscapeArtist>(out _) && PDHelper.IsInvincible)
                x += EnduranceLevel / 4f;
            if (HasPower<ImprovedSprintmaster>(out _))
                x += 3f;
            return x;
        });
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 2f;
            if (HasPower<EscapeArtist>(out _) && PDHelper.IsInvincible)
                x += EnduranceLevel / 4f;
            if (HasPower<ImprovedSprintmaster>(out _))
                x += 3f;
            return x;
        });
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 2f;
            if (HasPower<EscapeArtist>(out _) && PDHelper.IsInvincible)
                x += EnduranceLevel / 4f;
            return x;
        });
    }

    private static void ModifyAttackSpeed(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME_CH)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 0.8f;
            if (HasPower(out MercilessPursuit pursuit))
                x -= Math.Min(pursuit.Stacks, 10) * 0.01f;
            return x;
        });
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 0.7f;
            if (HasPower(out MercilessPursuit pursuit))
                x -= Math.Min(pursuit.Stacks, 10) * 0.016f;
            return x;
        });
    }

    private static void ModifyAttackDuration(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_DURATION_CH)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 0.8f;
            if (HasPower(out MercilessPursuit pursuit))
                x -= Math.Min(pursuit.Stacks, 10) * 0.0124f;
            return x;
        });

        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_DURATION)));
        // 10 stacks should match quick slash.
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 0.8f;
            if (HasPower(out MercilessPursuit pursuit))
                x -= Math.Min(pursuit.Stacks, 10) * 0.015f;
            return x;
        });
    }

    private static void ModifyOtherDealtDamage(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, TakeDamage self)
    {
        int vanillaDamage = self.DamageDealt.Value;
        bool isNailArtModifier = false;
        try
        {
            isNailArtModifier = self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name == "Great Slash" || self.Fsm.GameObject.name == "Dash Slash")
                && HasPower<Recklessness>(out _);
            if (self.IsCorrectContext("damages_enemy", null, "Send Event") && HasPower(out BurstingSoul soul))
            {
                string gameObjectName = self.Fsm.GameObjectName;
                string parentName = self.Fsm.GameObject.transform.parent?.name;
                if (gameObjectName.Contains("Fireball")
                    || gameObjectName == "Q Fall Damage"
                    || gameObjectName == "Hit U" && (parentName == "Scr Heads" || parentName == "Scr Heads 2")
                    || (gameObjectName == "Hit R" || gameObjectName == "Hit L") && (parentName == "Q Slam" || parentName == "Q Slam 2" || parentName == "Q Mega" || parentName == "Scr Heads" || parentName == "Scr Heads 2"))
                    self.DamageDealt = Math.Max(1, Mathf.FloorToInt(self.DamageDealt.Value * (2.2f - soul.SpellCount * 0.2f)));
            }
            else if (isNailArtModifier)
                self.DamageDealt.Value *= 5;
            else if (self.IsCorrectContext("damages_enemy", "SuperDash Damage", "Send Event") && HasPower<ImprovedCrystalDash>(out _))
                self.DamageDealt.Value += 20 + CombatLevel * 5;
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify dealt damage (other)", ex);
        }
        orig(self);
        self.DamageDealt.Value = vanillaDamage;
        try
        {
            if (isNailArtModifier)
            {
                HealthManager enemy = self.Target.Value.GetComponent<HealthManager>() ?? self.Target.Value.GetComponentInChildren<HealthManager>();
                if (enemy?.isDead == false)
                    HeroController.instance.TakeDamage(self.Target.Value, GlobalEnums.CollisionSide.top, 1, 1);
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify dealt damage (other)", ex);
        }
    }

    private static int ModifySoulGain(int amount)
    {
        try
        {
            amount = Math.Max(1, amount - 8 + SpiritLevel);
            if (HasPower(out Versatility versatility) && versatility.CastedSpell)
                amount += 2 + (SpiritLevel + CombatLevel) / 8;
            if (HasPower(out Powers.Common.Caching caching))
            {
                int excessiveAmount = (PDHelper.MaxMP + PDHelper.MPReserveMax - PDHelper.MPCharge - PDHelper.MPReserve - amount) * -1;
                if (excessiveAmount > 0 && RngManager.GetRandom(0, 50) <= SpiritLevel)
                {
                    if (caching.ActiveSoulCache != null)
                        caching.ActiveSoulCache.GetComponent<SoulCache>().SoulAmount += excessiveAmount;
                    else
                    {
                        caching.ActiveSoulCache = UnityEngine.Object.Instantiate(Powers.Common.Caching.SoulCache);
                        caching.ActiveSoulCache.transform.position = HeroController.instance.transform.position;
                        BoxCollider2D collider = caching.ActiveSoulCache.AddComponent<BoxCollider2D>();
                        collider.size = new(1.4f, 1.4f);
                        collider.isTrigger = true;
                        caching.ActiveSoulCache.layer = 18;
                        caching.ActiveSoulCache.AddComponent<SoulCache>().SoulAmount = excessiveAmount;
                        caching.ActiveSoulCache.SetActive(true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify soul gain", ex);
        }
        return amount;
    }

    private static void ModifyTakenDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject sourceObject, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        int currentHealth = PDHelper.Health;
        try
        {
            if (damageAmount != InstaKillDamage)
            {
                // Enemy scaling
                if (hazardType == 1)
                    damageAmount += StageController.CurrentRoomNumber / 20 * damageAmount;
                else
                    damageAmount += StageController.CurrentRoomNumber / 15;

                if (HasPower<AchillesVerse>(out _))
                {
                    if (hazardType > 1 && hazardType < 5)
                        damageAmount = InstaKillDamage;
                    else
                        damageAmount = damageAmount.LowerPositive(1 + Mathf.CeilToInt(EnduranceLevel / 8f));
                }

                if (hazardType > 1 && hazardType < 5)
                {
                    if (HasPower<ImprovedCaringShell>(out _))
                    {
                        if (!InCombat)
                            damageAmount = 0;
                        else
                            damageAmount = damageAmount.LowerPositive(2 + EnduranceLevel / 4);
                    }
                    else if (HasPower<CaringShell>(out _))
                        damageAmount = damageAmount.LowerPositive(1);
                }

                if (HasPower<Sturdy>(out _))
                    damageAmount = damageAmount.LowerPositive(1);

                if (HasPower<ShiningBound>(out _))
                    damageAmount = Mathf.CeilToInt(damageAmount / 2f);
                if (sourceObject != null)
                {
                    HealthManager enemyObject = sourceObject.GetComponent<HealthManager>();
                    if (enemyObject == null && sourceObject.transform.parent != null)
                        enemyObject = sourceObject.GetComponentInParent<HealthManager>();
                    if (enemyObject != null && enemyObject.GetComponent<WeakenedEffect>())
                        damageAmount = Mathf.FloorToInt(damageAmount * (DebuffsStronger ? 0.3f : 0.6f));
                }
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify taken damage", ex);
        }
        orig(self, sourceObject, damageSide, damageAmount, hazardType);
        try
        {
            if (currentHealth != PDHelper.Health)
            {
                if (HasPower(out FragileGreed greed) && greed.GreedActive || HasPower(out FragileSpirit spirit) && spirit.SpiritActive
                    || HasPower(out FragileStrength strength) && strength.StrengthActive)
                    HeroController.instance.proxyFSM.GetState("Flower?").GetFirstAction<ActivateGameObject>().gameObject.GameObject.Value.SetActive(true);
                TookDamage?.Invoke();
            }
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to calculate taken damage.", ex);
        }
    }

    private static void ModifyGeoAdd(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
    {
        try
        {
            if (HasPower(out FragileGreed greed) && greed.GreedActive)
                amount *= 3;
            // Greed works differently and should overwrite Interest.
            if (HasPower<Interest>(out _) && !HasPower<Greed>(out _))
                amount = Mathf.CeilToInt(amount * 1.2f);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify geo gain.", ex);
        }
        orig(self, amount);
    }

    private static IEnumerator OnPlayerDeath(On.HeroController.orig_Die orig, HeroController self)
    {
        if (!HasPower(out CheatDeath cheatDeath) || cheatDeath.Cooldown != 0 && RngManager.GetRandom(1, 21) >= EnduranceLevel + 1)
            yield return orig(self);
        else
        {
            cheatDeath.Cooldown = 10;
            // This only restores 2 without endurance points. I have no clue.
            HeroController.instance.AddHealth(3 + EnduranceLevel / 2);
            HeroController.instance.StartCoroutine(UpdateUI());
        }
    }

    #endregion

    private static void PassHistoryData(HistoryData entry, Enums.RunResult state)
    {
        entry.FinalCombatLevel = CombatLevel;
        entry.FinalSpiritLevel = SpiritLevel;
        entry.FinalEnduranceLevel = EnduranceLevel;
        entry.CommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Common);
        entry.UncommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Uncommon);
        entry.RarePowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Rare);
        entry.Powers = [.. ObtainedPowers.Select(x => x.Name)];
    }

    private static void FsmEdits(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
    {
        try
        {
            if (self.FsmName == "health_display")
            {
                self.AddState("Check for fill", () =>
                {
                    int healthNumber = int.Parse(self.gameObject.name.Substring("Health ".Length));
                    if (PDHelper.Health >= healthNumber)
                        self.SendEvent("HERO HEALED");
                }, FsmTransitionData.FromTargetState("Idle").WithEventName("HERO HEALED"),
                   FsmTransitionData.FromTargetState("Empty").WithEventName("FINISHED"));
                FsmState state = self.GetState("Check for fill");
                if (state.Actions.Length != 4)
                {
                    state.AddActions(self.GetState("Break?").GetActions<SetMeshRenderer>());
                    state.AddActions(self.GetState("Break?").GetActions<Tk2dPlayAnimationWithEvents>());
                    self.GetState("Max Up Flash").AdjustTransitions("Check for fill");
                }
            }
            // Trigger the hall of god version of radiance also adjust phase health to balance the phase duration (the other end bosses calculate the phases themselves).
            else if (self.gameObject.name == "Absolute Radiance")
            {
                if (self.FsmName == "Control")
                {
                    self.GetState("Tendrils 2").GetFirstAction<GGCheckIfBossSequence>().trueEvent = self.GetState("Tendrils 2").GetFirstAction<GGCheckIfBossSequence>().falseEvent;
                    self.GetState("Statue Death 2").AdjustTransitions("Return to workshop");
                }
                else if (self.FsmName == "Phase Control" && StageController.CurrentRoomNumber >= 20)
                {
                    self.FsmVariables.FindFsmInt("P2 Spike Waves").Value = Mathf.CeilToInt(self.FsmVariables.FindFsmInt("P2 Spike Waves").Value * (1 + (StageController.CurrentRoomNumber - 20) * 0.05f));
                    self.FsmVariables.FindFsmInt("P3 A1 Rage").Value = Mathf.CeilToInt(self.FsmVariables.FindFsmInt("P2 Spike Waves").Value * (1 + (StageController.CurrentRoomNumber - 20) * 0.05f));
                    self.FsmVariables.FindFsmInt("P4 Stun1").Value = Mathf.CeilToInt(self.FsmVariables.FindFsmInt("P2 Spike Waves").Value * (1 + (StageController.CurrentRoomNumber - 20) * 0.05f));
                    self.FsmVariables.FindFsmInt("P5 Acend").Value = Mathf.CeilToInt(self.FsmVariables.FindFsmInt("P2 Spike Waves").Value * (1 + (StageController.CurrentRoomNumber - 20) * 0.002f));
                }
            }
            else if (self.gameObject.name == "Brothers" || self.gameObject.name == "Nightmare Grimm Boss"
                || self.gameObject.name == "Mantis Battle" || self.gameObject.name == "Sly Boss")
            {
                if (self.FsmName.Contains("Control") && !self.FsmName.Contains("Stun"))
                {
                    if (self.gameObject.name != "Nightmare Grimm Boss")
                        self.GetState("Bow").InsertActions(0, () =>
                        {
                            GameObject crowd = GameObject.Find("Godseeker Crowd");
                            TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(crowd.transform.position.x, crowd.transform.position.y - 1f, HeroController.instance.transform.position.z), false);
                        });
                    else
                        self.GetState("Send NPC Event").AddActions(() =>
                        {
                            if (StageController.CurrentRoomNumber == StageController.CurrentRoomData.Count)
                                TrialOfCrusaders.Holder.StartCoroutine(StageController.WaitForTransition());
                            else
                            {
                                GameObject crowd = GameObject.Find("Godseeker Crowd");
                                TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(crowd.transform.position.x, crowd.transform.position.y - 1f, HeroController.instance.transform.position.z), false);
                            }
                        });
                }
            }
            // Prevent the small centipedes from moving away.
            else if (self.FsmName == "Dig Away" && self.gameObject.name.Contains("Baby Centipede"))
                self.GetState("Dig").RemoveFirstAction<SetFsmBool>();
        }
        catch (Exception ex)
        {
            LogManager.Log($"Failed to modify fsm. Fsm name: {self.FsmName}. Game object name {self.gameObject.name}", ex);
        }
        orig(self);
    }

    private static void StageController_RoomEnded(bool obj, bool traversed) => _stageTreasures = 0;
}
