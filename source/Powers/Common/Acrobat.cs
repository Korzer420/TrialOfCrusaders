using KorzUtils.Helper;
using System.Collections;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Acrobat : Power
{
    public bool Buff { get; set; }

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => PDHelper.HasDash;

    public override StatScaling Scaling => StatScaling.Combat;

    protected override void Enable() => On.HeroController.FinishedDashing += HeroController_FinishedDashing;

    protected override void Disable() => On.HeroController.FinishedDashing -= HeroController_FinishedDashing;

    private IEnumerator BuffTime()
    {
        Buff = true;
        float passedTime = 0f;
        while (passedTime <= 0.25f)
        {
            yield return null;
            passedTime += Time.deltaTime;
        }
        Buff = false;
    }

    private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
    {
        StartRoutine(BuffTime());
        orig(self);
    }
}
