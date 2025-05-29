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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents;
using TrialOfCrusaders.UnityComponents.Debuffs;
using UnityEngine;

namespace TrialOfCrusaders.Controller;

public static class CombatController
{
    private static bool _enabled;
    private static ILHook _attackMethod;
    private static Coroutine _enemyScanner;

    public const int InstaKillDamage = 500;

    /// <summary>
    /// Gets or sets the combat level.
    /// Each one increases the nail damage by two (except the last two, which grant 3 instead).
    /// </summary>
    public static int CombatLevel { get; set; }

    /// <summary>
    /// Gets or sets the spirit level.
    /// Each one increases the max amount of soul the player has by 8 (except the last two which grant 10 and 11 respectively)
    /// </summary>
    public static int SpiritLevel { get; set; }

    /// <summary>
    /// Gets or sets the survival level.
    /// Each one grants one extra mask.
    /// </summary>
    public static int EnduranceLevel { get; set; }

    public static bool CharmUpdate { get; set; }

    public static List<HealthManager> Enemies { get; set; } = [];

    public static List<Power> ObtainedPowers { get; set; } = [];

    public static bool InCombat { get; set; }

    public static event Action TookDamage;

    public static event Action EnemiesCleared;

    public static event Action<HealthManager> EnemyKilled;

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

    #region Setup

    public static void Initialize()
    {
        if (_enabled)
            return;
        LogHelper.Write<TrialOfCrusaders>("Enable Combat Controller", KorzUtils.Enums.LogType.Debug);

        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HealthManager.OnEnable += HealthManager_OnEnable;
        On.HealthManager.Die += HealthManager_Die;
        IL.HeroController.Move += HeroController_Move;
        IL.HeroController.Attack += HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        On.HeroController.AddGeo += HeroController_AddGeo;
        IL.HeroController.MaxHealth += BlockFullHeal;
        On.HeroController.CharmUpdate += HeroController_CharmUpdate;
        On.HeroController.FinishedEnteringScene += HeroController_FinishedEnteringScene;
        On.HeroController.Die += HeroController_Die;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter += IntSwitch_OnEnter;
        On.GameManager.GetCurrentMapZone += GameManager_GetCurrentMapZone;
        //ModHooks.OnEnableEnemyHook += ModHooks_OnEnableEnemyHook;
        _attackMethod = new(typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance), ModifyAttackSpeed);
        HistoryController.CreateEntry += HistoryController_CreateEntry;
        // This is called upon leaving a godhome room and would restore the health + remove lifeblood.
        IL.BossSequenceController.RestoreBindings += BlockHealthReset;
        On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.OnEnter += AdjustLifebloodPosition;
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter += MoveLifebloodInFront;
        On.BossSceneController.Start += BossSceneController_Start;

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
        LogHelper.Write<TrialOfCrusaders>("Disable Combat Controller", KorzUtils.Enums.LogType.Debug);
        On.PlayMakerFSM.OnEnable -= PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
        ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
        On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
        On.HealthManager.OnEnable -= HealthManager_OnEnable;
        On.HealthManager.Die -= HealthManager_Die;
        IL.HeroController.Move -= HeroController_Move;
        IL.HeroController.Attack -= HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter -= TakeDamage_OnEnter;
        ModHooks.SoulGainHook -= ModHooks_SoulGainHook;
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        On.HeroController.AddGeo -= HeroController_AddGeo;
        IL.HeroController.MaxHealth -= BlockFullHeal;
        On.HeroController.CharmUpdate -= HeroController_CharmUpdate;
        On.HeroController.FinishedEnteringScene -= HeroController_FinishedEnteringScene;
        On.HeroController.Die -= HeroController_Die;
        On.HutongGames.PlayMaker.Actions.IntSwitch.OnEnter -= IntSwitch_OnEnter;
        On.GameManager.GetCurrentMapZone -= GameManager_GetCurrentMapZone;
        //ModHooks.OnEnableEnemyHook -= ModHooks_OnEnableEnemyHook;
        _attackMethod?.Dispose();
        HistoryController.CreateEntry -= HistoryController_CreateEntry;
        IL.BossSequenceController.RestoreBindings -= BlockHealthReset;
        On.HutongGames.PlayMaker.Actions.ConvertIntToFloat.OnEnter -= AdjustLifebloodPosition;
        On.HutongGames.PlayMaker.Actions.SetPosition.OnEnter -= MoveLifebloodInFront;
        On.BossSceneController.Start -= BossSceneController_Start;
        _enabled = false;

        // Reset data.
        CombatLevel = 0;
        SpiritLevel = 0;
        EnduranceLevel = 0;
        InCombat = false;
        CharmUpdate = false;
        DisablePowers();
        ObtainedPowers.Clear();
        if (_enemyScanner != null)
            TrialOfCrusaders.Holder.StopCoroutine(_enemyScanner);
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

    private static void PlayMakerFSM_OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self)
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
                self.FsmVariables.FindFsmInt("P5 Ascent").Value = Mathf.CeilToInt(self.FsmVariables.FindFsmInt("P2 Spike Waves").Value * (1 + (StageController.CurrentRoomNumber - 20) * 0.002f));
            }
        }
        else if (self.gameObject.name == "Brothers" || self.gameObject.name == "Nightmare Grimm Boss" 
            || self.gameObject.name == "Mantis Battle" || self.gameObject.name == "Sly Boss")
        {
            if (self.FsmName.Contains("Control"))
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
                        GameObject crowd = GameObject.Find("Godseeker Crowd");
                        TreasureManager.SpawnShiny(TreasureType.NormalOrb, new(crowd.transform.position.x, crowd.transform.position.y - 1f, HeroController.instance.transform.position.z), false);
                    });
            }
        }
        orig(self);
    }

    private static void HeroController_CharmUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
    {
        CharmUpdate = true;
        orig(self);
        CharmUpdate = false;
    }

    private static void BlockFullHeal(ILContext il)
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
        HeroController.instance.proxyFSM.SendEvent("HeroCtrl-Healed");
    }

    private static void IntSwitch_OnEnter(On.HutongGames.PlayMaker.Actions.IntSwitch.orig_OnEnter orig, IntSwitch self)
    {
        if (self.IsCorrectContext("Hero Death Anim", "Hero Death", "Check MP"))
        {
            // Reset shade, save history entry and reset to hub control.
            PDHelper.ShadeMapZone = string.Empty;
            PDHelper.ShadeScene = string.Empty;
            PDHelper.ShadePositionX = 0;
            PDHelper.ShadePositionY = 0;
            PDHelper.SoulLimited = false;
            self.Fsm.Variables.FindFsmBool("Soul Cracked").Value = false;
            ScoreController.Score.Score = PDHelper.GeoPool;
            HistoryController.AddEntry(RunResult.Failed);
            PDHelper.GeoPool = 0;
            Unload();
            ScoreController.Unload();
            StageController.Unload();
            HubController.Initialize();
            HistoryController.Initialize();
        }
        orig(self);
    }

    private static string GameManager_GetCurrentMapZone(On.GameManager.orig_GetCurrentMapZone orig, GameManager self)
    {
        orig(self);
        return "COLOSSEUM";
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
        {
            self.floatVariable.Value = PDHelper.HealthBlue;
            LogHelper.Write("Amount: " + self.floatVariable.Value);
        }
    }

    #endregion

    #region Enemy Control

    private static void HealthManager_OnEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
    {
        // Prevent "immortal" enemies.
        if (self.hp != 9999 && self.gameObject.name != "Mender Bug" && !self.gameObject.name.Contains("Pigeon") && !self.gameObject.name.Contains("Hatcher Baby Spawner")
            && self.gameObject.name != "Hollow Shade(Clone)" && !self.gameObject.name.Contains("fluke_baby") 
            && self.gameObject.name != "Cap Hit" && !self.gameObject.name.Contains("Baby Centipede Spawner") 
            && !self.gameObject.name.Contains("Zombie Spider") && !self.gameObject.name.Contains("Híveling Spawner"))
        {
            Enemies.Add(self);
            if (self.hp >= 190f && StageController.CurrentRoom.BossRoom)
                self.gameObject.AddComponent<BossFlag>();
            if (StageController.CurrentRoomNumber >= 20 && self.hp != 1)
            {
                float scaling = 0.1f;
                // Pure Vessel and NKG receive a greater scaling than other bosses as an attempt to match the difficulty with Radiance.
                if ((StageController.CurrentRoomNumber != 120 || StageController.CurrentRoomData[StageController.CurrentRoomIndex].Name == "GG_Radiance")
                    && StageController.CurrentRoomData[StageController.CurrentRoomIndex].BossRoom)
                    scaling = 0.05f;
                //self.hp = Mathf.CeilToInt(self.hp * (1 + (StageController.CurrentRoomNumber - 20) * scaling));
            }
        }
        orig(self);
    }

    private static void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        bool contained = false;
        try
        {
            contained = Enemies.Contains(self);
            Enemies.Remove(self);
            UpdateEnemies();
            if (contained && self.GetComponent<BaseEnemy>() is BaseEnemy enemyFlag)
            {
                EnemyKilled?.Invoke(self);
                if (!enemyFlag.NoLoot && !StageController.CurrentRoom.BossRoom)
                {
                    float rolled = RngProvider.GetStageRandom(0f, 100f);
                    if (rolled <= 4f)
                        TreasureManager.SpawnShiny(TreasureType.NormalOrb, self.transform.position);
                    else if (rolled <= 12f)
                    {
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
            LogHelper.Write("Error in HealthManager Die before orig", ex);
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
        try
        {
            if (Enemies.Count(x => x.GetComponent<BaseEnemy>()) == 0 && !StageController.QuietRoom && contained 
                && !StageController.FinishedEnemies && !StageController.CurrentRoom.BossRoom)
            {
                InCombat = false;
                EnemiesCleared?.Invoke();
            }
        }
        catch (Exception ex)
        {
            LogHelper.Write("Error in HealthManager Die after orig", ex);
        }
    }

    private static void HeroController_FinishedEnteringScene(On.HeroController.orig_FinishedEnteringScene orig, HeroController self, bool setHazardMarker, bool preventRunBob)
    {
        orig(self, setHazardMarker, preventRunBob);
        Enemies ??= [];
        List<HealthManager> newEnemies = [];
        foreach (HealthManager item in Enemies)
            if (item != null && item.gameObject != null && item.gameObject.scene != null && item.gameObject.scene.name == GameManager.instance.sceneName)
            {
                if (StageController.CurrentRoom.BossRoom && item.GetComponent<BossFlag>())
                    item.OnDeath += Boss_OnDeath;
                item.gameObject.AddComponent<BaseEnemy>().NoLoot = item.hp == 1;
                newEnemies.Add(item);
            }
        if (!StageController.QuietRoom)
        { 
            InCombat = true;
            if (StageController.CurrentRoom.BossRoom)
                _bossCounter = GameManager.instance.sceneName switch
                {
                    "GG_Watcher_Knights" => 6,
                    "GG_Soul_Master" or "GG_Oblobbles" or "GG_Soul_Tyrant" or "GG_Vengefly_V" => 2,
                    _ => 1
                };
        }
    }

    private static bool ModHooks_OnEnableEnemyHook(GameObject enemy, bool isAlreadyDead) => false;

    private static IEnumerator ScanEnemies()
    {
        // I really, really, really, really, really, really, really, really, really, really, really wanted to avoid doing shit like this
        // But since the enemy system in this game is so fucking scuffed, we unfortunately have to do this to prevent issues.
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (StageController.QuietRoom)
                continue;
            UpdateEnemies();
        }
    }

    private static void UpdateEnemies()
    {
        int currentCount = Enemies.Count;
        // Unity doesn't like the "?" operator.
        for (int i = 0; i < Enemies.Count; i++)
            if (Enemies[i] == null || Enemies[i].gameObject == null || !Enemies[i].gameObject.activeSelf)
            {
                Enemies.RemoveAt(i);
                i--;
            }
        if (currentCount != Enemies.Count && Enemies.Count == 0 && !StageController.CurrentRoom.BossRoom)
            EnemiesCleared?.Invoke();
    }

    private static int _bossCounter = 0;

    private static IEnumerator BossSceneController_Start(On.BossSceneController.orig_Start orig, BossSceneController self)
    {
        foreach (HealthManager boss in self.bosses)
            if (!Enemies.Contains(boss))
            {
                boss.gameObject.AddComponent<BaseEnemy>();
                boss.gameObject.AddComponent<BossFlag>();
                Enemies.Add(boss);
                boss.OnDeath += Boss_OnDeath;
            }
        yield return orig(self);
    }

    private static void Boss_OnDeath()
    {
        _bossCounter--;
        if (_bossCounter == 0)
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
        // Block overcharm
        if (name == nameof(PlayerData.overcharmed))
            return false;
        else if (name == nameof(PlayerData.isInvincible))
            return orig || TreasureManager.SelectionActive;
        return orig;
    }

    private static int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        if (damageAmount == InstaKillDamage)
            return InstaKillDamage;
        if (HasPower(out PaleShell shell) && shell.Shielded)
        {
            shell.Shielded = false;
            return 0;
        }

        // Enemy scaling
        if (hazardType == 1)
            damageAmount += StageController.CurrentRoomNumber / 20 * damageAmount;
        else
            damageAmount += StageController.CurrentRoomNumber / 10;

        if (HasPower<AchillesVerse>(out _))
        {
            if (hazardType > 1 && hazardType < 5)
                return InstaKillDamage;
            else
                damageAmount = damageAmount.Lower(1 + Mathf.CeilToInt(EnduranceLevel / 8f));
            if (damageAmount == 0)
                return 0;
        }

        if (hazardType > 1 && hazardType < 5)
        {
            if (HasPower<ImprovedCaringShell>(out _))
            {
                if (!InCombat)
                    return 0;
                damageAmount = damageAmount.Lower(2 + EnduranceLevel / 4);
            }
            else if (HasPower<CaringShell>(out _))
                damageAmount = damageAmount.Lower(1);
            if (damageAmount == 0)
                return 0;
        }

        if (HasPower<Sturdy>(out _))
            damageAmount--;

        if (HasPower<ShiningBound>(out _))
            damageAmount = Mathf.CeilToInt(damageAmount / 2f);
        return damageAmount;
    }

    private static void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
        {
            if (HasPower(out FragileStrength strength) && strength.StrengthActive)
                hitInstance.DamageDealt *= 4;

            if (HasPower<Initiative>(out _) && self.GetComponent<InitiativeEffect>() == null)
            {
                self.gameObject.AddComponent<InitiativeEffect>();
                HeroController.instance.AddMPCharge(Math.Max(2, SpiritLevel / 2));
                hitInstance.DamageDealt += 20 + CombatLevel * 2;
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
                hitInstance.DamageDealt += 20 + CombatLevel * 2;
            }
        }

        if (self.GetComponent<ShatteredMindEffect>() is ShatteredMindEffect effect)
            hitInstance.DamageDealt += effect.ExtraDamage;

#if DEBUG
        hitInstance.DamageDealt = Math.Max(50, hitInstance.DamageDealt);
#endif
        orig(self, hitInstance);
    }

    private static void HeroController_Move(ILContext il)
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

    private static void HeroController_Attack(ILContext il)
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

    private static void TakeDamage_OnEnter(On.HutongGames.PlayMaker.Actions.TakeDamage.orig_OnEnter orig, TakeDamage self)
    {
        int vanillaDamage = self.DamageDealt.Value;
        bool isNailArtModifier = self.IsCorrectContext("damages_enemy", null, "Send Event") && (self.Fsm.GameObject.name == "Great Slash" || self.Fsm.GameObject.name == "Dash Slash")
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
        orig(self);
        self.DamageDealt.Value = vanillaDamage;
        if (isNailArtModifier)
        {
            HealthManager enemy = self.Target.Value.GetComponent<HealthManager>() ?? self.Target.Value.GetComponentInChildren<HealthManager>();
            if (enemy?.isDead == false)
                HeroController.instance.TakeDamage(self.Target.Value, GlobalEnums.CollisionSide.top, 1, 1);
        }
    }

    private static int ModHooks_SoulGainHook(int amount)
    {
        amount = SpiritLevel + 3;
        if (HasPower(out Versatility versatility) && versatility.CastedSpell)
            amount += 2 + (SpiritLevel + CombatLevel) / 8;
        if (HasPower(out Powers.Common.Caching caching))
        {
            int excessiveAmount = (PDHelper.MaxMP + PDHelper.MPReserveMax - PDHelper.MPCharge - PDHelper.MPReserve - amount) * -1;
            if (excessiveAmount > 0 && RngProvider.GetRandom(0, 50) <= SpiritLevel)
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
        return amount;
    }

    private static void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        int currentHealth = PDHelper.Health;
        orig(self, go, damageSide, damageAmount, hazardType);
        if (currentHealth != PDHelper.Health)
        {
            if (HasPower(out FragileGreed greed) && greed.GreedActive || HasPower(out FragileSpirit spirit) && spirit.SpiritActive
                || HasPower(out FragileStrength strength) && strength.StrengthActive)
            {
                HeroController.instance.proxyFSM.GetState("Flower?").GetFirstAction<ActivateGameObject>().gameObject.GameObject.Value.SetActive(true);
                GameManager.instance.SaveGame();
            }
            TookDamage?.Invoke();
        }
    }

    private static void HeroController_AddGeo(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
    {
        if (HasPower(out FragileGreed greed) && greed.GreedActive)
            amount *= 3;
        // Greed works differently and should overwrite Interest.
        if (HasPower<Interest>(out _) && !HasPower<Greed>(out _))
            amount = Mathf.CeilToInt(amount * 1.2f);
        orig(self, amount);
    }

    private static System.Collections.IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
    {
        if (!HasPower(out CheatDeath cheatDeath) || cheatDeath.Cooldown != 0 && RngProvider.GetRandom(1, 21) >= EnduranceLevel + 1)
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

    private static void HistoryController_CreateEntry(HistoryData entry, Enums.RunResult state)
    {
        entry.FinalCombatLevel = CombatLevel;
        entry.FinalSpiritLevel = SpiritLevel;
        entry.FinalEnduranceLevel = EnduranceLevel;
        entry.CommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Common);
        entry.UncommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Uncommon);
        entry.RarePowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Rare);
        entry.Powers = [.. ObtainedPowers.Select(x => x.Name)];
    }
}
