using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using KorzUtils.Data;
using KorzUtils.Helper;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders;

public static class CombatController
{
    private static ILHook _attackMethod;

    public const int InstaKillDamage = 500;

    public static string UpcomingPowerSelection { get; set; }

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

    public static List<HealthManager> Enemies => StageController.Enemies;

    public static List<Power> ObtainedPowers { get; set; } = [];

    public static event Action TookDamage;

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

    public static void Initialize()
    {
        On.PlayMakerFSM.OnEnable += PlayMakerFSM_OnEnable;
        ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;
        ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        IL.HeroController.Move += HeroController_Move;
        IL.HeroController.Attack += HeroController_Attack;
        On.HutongGames.PlayMaker.Actions.TakeDamage.OnEnter += TakeDamage_OnEnter;
        ModHooks.SoulGainHook += ModHooks_SoulGainHook;
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        On.HeroController.AddGeo += HeroController_AddGeo;
        IL.HeroController.MaxHealth += HeroController_MaxHealth;
        On.HeroController.CharmUpdate += HeroController_CharmUpdate;
        _attackMethod = new(typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance), ModifyAttackSpeed);
        CreateExtraHealth();
        CoroutineHelper.WaitUntil(() =>
        {
            // Adjust max health and max mp.
            int intendedMaxHealth = 5 + EnduranceLevel;
            if (PDHelper.MaxHealth != intendedMaxHealth)
            {
                PDHelper.MaxHealth += intendedMaxHealth - PDHelper.MaxHealth;
                HeroController.instance.MaxHealth();
            }

            int maxMP = 33 + (Math.Min(SpiritLevel, 18) * 8) + (SpiritLevel == 19 ? 10 : 0);
            int soulVessel = 0;
            if (maxMP >= 165)
                soulVessel = 3;
            else if (maxMP > 132)
                soulVessel = 2;
            else if (maxMP > 99)
                soulVessel = 1;
            PDHelper.MPReserveMax = soulVessel * 33;
            // To not deal with the UI, we just toggle it.
            GameCameras.instance.hudCanvas.gameObject.SetActive(false);
            GameCameras.instance.hudCanvas.gameObject.SetActive(true);
        }, () => HeroController.instance?.acceptingInput == true, true);
    }

    private static void HeroController_CharmUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
    {
        CharmUpdate = true;
        orig(self);
        CharmUpdate = false;
    }

    private static void HeroController_MaxHealth(ILContext il)
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

    public static void Unload()
    {
        ModHooks.GetPlayerIntHook -= ModHooks_GetPlayerIntHook;
        ModHooks.GetPlayerBoolHook -= ModHooks_GetPlayerBoolHook;
    }

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
            GameObject gameObject = GameObject.Instantiate(healthPrefab, cameraObject.Find("HudCamera/Hud Canvas/Health"));
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
        orig(self);
    }

    #endregion

    #region Global Event Handler

    private static int ModHooks_GetPlayerIntHook(string name, int orig)
    {
        if (name == nameof(PlayerData.instance.nailDamage))
            orig = 3 + (CombatLevel * 2) + (CombatLevel >= 19 ? 1 : 0);
        else if (name == nameof(PlayerData.maxHealthCap))
            return 25;
        else if (name == nameof(PlayerData.maxHealthBase) || name == nameof(PlayerData.maxHealth))
            return 5 + EnduranceLevel;
        else if (name == nameof(PlayerData.maxMP))
            return Math.Min(99, 33 + (Math.Min(SpiritLevel, 18) * 8) + (SpiritLevel == 19 ? 10 : 0));
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
        if (name == nameof(PlayerData.instance.overcharmed))
            return false;
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
                if (!StageController.InCombat)
                    return 0;
                damageAmount = damageAmount.Lower(2 + (EnduranceLevel / 4));
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

            if (self.GetComponent<InitiativeEffect>() == null)
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
            if (hitInstance.AttackType == AttackTypes.Spell)
                hitInstance.DamageDealt += SpiritLevel * 2;
            if (self.GetComponent<InitiativeEffect>() == null)
            {
                self.gameObject.AddComponent<InitiativeEffect>();
                HeroController.instance.AddMPCharge(Math.Max(2, SpiritLevel / 2));
                hitInstance.DamageDealt += 20 + CombatLevel * 2;
            }
        }

        if (self.GetComponent<ShatteredMindEffect>() is ShatteredMindEffect effect)
            hitInstance.DamageDealt += effect.ExtraDamage;

        orig(self, hitInstance);
    }

    private static void HeroController_Move(MonoMod.Cil.ILContext il)
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
                || (gameObjectName == "Hit U" && (parentName == "Scr Heads" || parentName == "Scr Heads 2"))
                || ((gameObjectName == "Hit R" || gameObjectName == "Hit L") && (parentName == "Q Slam" || parentName == "Q Slam 2" || parentName == "Q Mega" || parentName == "Scr Heads" || parentName == "Scr Heads 2")))
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
                    caching.ActiveSoulCache = GameObject.Instantiate(Powers.Common.Caching.SoulCache);
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
            if ((HasPower(out FragileGreed greed) && greed.GreedActive) || (HasPower(out FragileSpirit spirit) && spirit.SpiritActive)
                || (HasPower(out FragileStrength strength) && strength.StrengthActive))
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

    #endregion

}
