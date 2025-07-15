using KorzUtils.Helper;
using Modding;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Manager;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;
using TrialOfCrusaders.Powers.Uncommon;
using TrialOfCrusaders.SaveData;
using TrialOfCrusaders.UnityComponents.PowerElements;
using UnityEngine;
using static TrialOfCrusaders.ControllerShorthands;

namespace TrialOfCrusaders.Controller;

public class PowerController : BaseController, ISaveData
{
    #region Members

    private ILHook _blueHealthMethod;

    #endregion

    #region Properties

    public bool CharmUpdate { get; set; }

    // As this is called very frequently we store it in an extra value.
    internal bool DebuffsStronger { get; set; }

    public List<Power> ObtainedPowers { get; set; } = [];

    #endregion

    #region Utils

    public bool HasPower<T>(out T selectedPower) where T : Power
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

    public bool HasPower(string powerTypeName)
    {
        foreach (Power power in ObtainedPowers)
            if (power.GetType().Name == powerTypeName)
                return true;
        return false;
    }

    public bool HasNailArt() => HasPower<DashSlash>(out _) || HasPower<GreatSlash>(out _) || HasPower<CycloneSlash>(out _);

    public bool HasSpell() => HasPower<VengefulSpirit>(out _) || HasPower<HowlingWraiths>(out _) || HasPower<DesolateDive>(out _);

    internal void DisablePowers()
    {
        foreach (Power power in ObtainedPowers)
            power.DisablePower();
    }

    #endregion

    public override Phase[] GetActivePhases() => [Phase.Run];

    protected override void Enable()
    {
        HistoryRef.CreateEntry += PassHistoryData;
        ModHooks.SoulGainHook += ModifySoulGain;
        On.HeroController.AddGeo += ModifyGeoAdd;
        On.HeroController.CharmUpdate += FlagCharmHeal;
        IL.HeroController.MaxHealth += BlockCharmHeal;
        IL.HeroController.Move += ModifyMovementSpeed;
        _blueHealthMethod = new(typeof(PlayerData).GetMethod("orig_UpdateBlueHealth", BindingFlags.Public | BindingFlags.Instance), SkipBlueHealthReset);
    }

    protected override void Disable()
    {
        HistoryRef.CreateEntry -= PassHistoryData;
        ModHooks.SoulGainHook -= ModifySoulGain;
        On.HeroController.AddGeo -= ModifyGeoAdd;
        On.HeroController.CharmUpdate -= FlagCharmHeal;
        IL.HeroController.MaxHealth -= BlockCharmHeal;
        IL.HeroController.Move -= ModifyMovementSpeed;
        _blueHealthMethod?.Dispose();
        DisablePowers();
        ObtainedPowers.Clear();
    }

    public void ReceiveSaveData(LocalSaveData saveData)
    {
        if (saveData.RetainData.ContainsKey("Banish"))
            TreasureManager.GetPower<Banish>().BanishedScene = saveData.RetainData["Banish"];
    }

    public void UpdateSaveData(LocalSaveData saveData)
    {
        if (HasPower(out Banish banish))
        {
            (TreasureManager.Powers.First(x => x.GetType() == typeof(Banish)) as Banish).BanishedScene = banish.BanishedScene;
            if (!saveData.RetainData.ContainsKey("Banish"))
                saveData.RetainData.Add("Banish", banish.BanishedScene);
            else
                saveData.RetainData["Banish"] = banish.BanishedScene;
        }
    }

    #region Eventhandler

    private void FlagCharmHeal(On.HeroController.orig_CharmUpdate orig, HeroController self)
    {
        // Flag the charm update so the heal can be blocked.
        CharmUpdate = true;
        orig(self);
        CharmUpdate = false;
    }

    private void SkipBlueHealthReset(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.Emit(OpCodes.Ret);
    }

    private void BlockCharmHeal(ILContext il)
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

    private void ModifyMovementSpeed(ILContext il)
    {
        ILCursor cursor = new(il);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.RUN_SPEED_CH_COMBO)));
        cursor.EmitDelegate<Func<float, float>>(x =>
        {
            if (HasPower(out Charge charge) && charge.Active)
                x *= 2f;
            if (HasPower<EscapeArtist>(out _) && PDHelper.IsInvincible)
                x += CombatRef.EnduranceLevel / 4f;
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
                x += CombatRef.EnduranceLevel / 4f;
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
                x += CombatRef.EnduranceLevel / 4f;
            return x;
        });
    }

    private int ModifySoulGain(int amount)
    {
        try
        {
            amount = Math.Max(1, amount - 8 + CombatRef.SpiritLevel);
            if (HasPower(out Versatility versatility) && versatility.CastedSpell)
                amount += 2 + (CombatRef.SpiritLevel + CombatRef.CombatLevel) / 8;
            if (HasPower(out Powers.Common.Caching caching))
            {
                int excessiveAmount = (PDHelper.MaxMP + PDHelper.MPReserveMax - PDHelper.MPCharge - PDHelper.MPReserve - amount) * -1;
                if (excessiveAmount > 0 && RngManager.GetRandom(0, 50) <= CombatRef.SpiritLevel)
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

    private void ModifyGeoAdd(On.HeroController.orig_AddGeo orig, HeroController self, int amount)
    {
        try
        {
            if (HasPower(out FragileGreed greed) && greed.GreedActive)
                amount *= 3;
            // Greed works differently and should overwrite Interest.
            if (HasPower<Interest>(out _))
                amount = Mathf.FloorToInt(amount * 1.2f);
        }
        catch (Exception ex)
        {
            LogManager.Log("Failed to modify geo gain.", ex);
        }
        orig(self, amount);
    }

    private void PassHistoryData(HistoryData entry, RunResult arg2)
    {
        entry.CommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Common);
        entry.UncommonPowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Uncommon);
        entry.RarePowerAmount = ObtainedPowers.Count(x => x.Tier == Rarity.Rare);
        entry.Powers = [.. ObtainedPowers.Select(x => x.Name)];
    }

    #endregion
}
