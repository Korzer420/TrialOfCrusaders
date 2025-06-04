using KorzUtils.Helper;
using System;
using System.Collections;
using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class CalmMind : Power
{
    private Coroutine _coroutine;

    public override (float, float, float) BonusRates => new(0f, 10f, 0f);

    public override StatScaling Scaling => StatScaling.Spirit;

    protected override void Enable() => _coroutine = StartRoutine(CheckForHealth());

    protected override void Disable()
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
