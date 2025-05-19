using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Revenge : Power
{
    public override string Name => "Revenge";

    public override string Description => "Killing an enemy that dealt damage to you recently restores a bit of health.";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    protected override void Enable()
    {
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        On.HealthManager.Die += HealthManager_Die;
    }

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        if (self.GetComponent<RevengeEffect>() is RevengeEffect revengeEffect)
        {
            Object.Destroy(revengeEffect);
            HeroController.instance.AddHealth(1 + (CombatController.CombatLevel + CombatController.EnduranceLevel) / 8);
        }
        orig(self, attackDirection, attackType, ignoreEvasion);
    }

    private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
    {
        orig(self, go, damageSide, damageAmount, hazardType);
        if (damageAmount > 0 && (go.GetComponent<HealthManager>() is not null || go.GetComponentInParent<HealthManager>() is not null))
        {
            HealthManager enemy = go.GetComponent<HealthManager>() ?? go.GetComponentInParent<HealthManager>();
            enemy.gameObject.AddComponent<RevengeEffect>();
        }
    }

    protected override void Disable()
    {

    }
}
