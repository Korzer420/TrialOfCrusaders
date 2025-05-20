using GlobalEnums;
using Modding;
using MonoMod.RuntimeDetour;
using System.Collections;
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

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        _duration = 0.5f;
        Stacks = 0;
        ModHooks.AttackHook += ModHooks_AttackHook;
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
