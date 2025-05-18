using KorzUtils.Helper;
using System;
using System.Collections;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Untouchable : Power
{
    private Coroutine _coroutine;

    public override string Name => "Untouchable";

    public override string Description => "While at full health, slowing regenerates soul.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable() => _coroutine = StartRoutine(CheckForHealth());

    internal override void Disable()
    {
        if (_coroutine != null)
            StopRoutine(_coroutine);
    }

    private IEnumerator CheckForHealth()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            if (PDHelper.Health == PDHelper.MaxHealth)
                HeroController.instance.AddMPCharge(Math.Max(3, CombatController.SpiritLevel / 2));
        }
    }
}
