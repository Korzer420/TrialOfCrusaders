using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedStalwartShell : Power
{
    private ILHook _hook;

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<StalwartShell>();

    public override StatScaling Scaling => StatScaling.Endurance;

    protected override void Enable()
        => _hook = new(typeof(HeroController).GetMethod("StartRecoil", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), IL_StartRecoil);

    protected override void Disable() => _hook?.Dispose();

    private void IL_StartRecoil(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>("INVUL_TIME_STAL"));
        cursor.EmitDelegate<Func<float, float>>(x => x * (1 + CombatController.EnduranceLevel / 20f));
    }
}
