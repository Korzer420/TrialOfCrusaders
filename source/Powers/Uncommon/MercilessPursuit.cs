using GlobalEnums;
using KorzUtils.Helper;
using Modding;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class MercilessPursuit : Power
{
    private ILHook _attackMethod;
    private Coroutine _coroutine;
    // 1 up, 2 down, 3 left, 4 right
    private int _attackDirection;
    private float _duration = 0.5f;

    public int Stacks { get; set; }

    public override string Name => "Merciless Pursuit";

    public override string Description => "Attacks in the same direction within half a second second grant stacking attack speed.";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        _duration = 0.5f;
        Stacks = 0;
        ModHooks.AttackHook += ModHooks_AttackHook;
        _attackMethod = new(typeof(HeroController).GetMethod("orig_DoAttack", BindingFlags.NonPublic | BindingFlags.Instance), ModifyAttackSpeed);
    }

    protected override void Disable()
    {
        ModHooks.AttackHook += ModHooks_AttackHook;
        _attackMethod?.Dispose();
    }

    private void ModHooks_AttackHook(AttackDirection direction)
    {
        int struckDirection;
        if (direction == AttackDirection.normal)
            struckDirection = HeroController.instance.cState.facingRight ? 4 : 3;
        else
            struckDirection = (int)direction;
        if (_attackDirection == struckDirection)
        {
            Stacks++;
            if (_coroutine == null)
                _coroutine = HeroController.instance.StartCoroutine(Cooldown());
            else
                _duration = 0.5f;
        }
        else
        {
            Stacks = 0;
            if (_coroutine != null)
                HeroController.instance.StopCoroutine(_coroutine);
        }
        _attackDirection = struckDirection;
    }

    private void ModifyAttackSpeed(ILContext context)
    {
        ILCursor cursor = new(context);
        cursor.Goto(0);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME_CH)));
        cursor.EmitDelegate<Func<float, float>>(x => x - Math.Min(Stacks, 10) * 0.015f);
        cursor.GotoNext(MoveType.After, x => x.MatchLdfld<HeroController>(nameof(HeroController.ATTACK_COOLDOWN_TIME)));
        // 10 stacks should match quick slash.
        cursor.EmitDelegate<Func<float, float>>(x => 
            x - Math.Min(Stacks, 10) * 0.0151f);
    }

    private IEnumerator Cooldown()
    {
        _duration = 0.5f;
        while (_duration >= 0f)
        {
            yield return null;
            _duration -= Time.deltaTime;
        }
        Stacks = 0;
    }
}
