using System.Collections;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Acrobat : Power
{
    private bool _buff;

    public override string Name => "Acrobat";

    public override string Description => "A nail hit right after dashing does increased damage and ignores armor.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    { 
        On.HealthManager.TakeDamage += HealthManager_TakeDamage;
        On.HeroController.FinishedDashing += HeroController_FinishedDashing;
    }

    private void HeroController_FinishedDashing(On.HeroController.orig_FinishedDashing orig, HeroController self)
    {
        StartRoutine(BuffTime());
        orig(self);
    }

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (_buff && hitInstance.AttackType == AttackTypes.Nail)
            hitInstance.DamageDealt += 50 /*Mathf.FloorToInt(CombatController.CombatPower * 1.5f)*/;
        orig(self, hitInstance);
    }

    internal override void Disable() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

    private IEnumerator BuffTime()
    {
        _buff = true;
        float passedTime = 0f;
        while(passedTime <= 0.25f)
        {
            yield return null;
            passedTime += Time.deltaTime;
        }
        _buff = false;
    }
}
