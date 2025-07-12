using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents.PowerElements;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Common;

internal class Revenge : Power
{
    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    public override StatScaling Scaling => StatScaling.Combat | StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Endurance;

    protected override void Enable()
    {
        On.HeroController.TakeDamage += HeroController_TakeDamage;
        CombatRef.EnemyKilled += CombatController_EnemyKilled;
    }

    private void CombatController_EnemyKilled(HealthManager enemy)
    {
        if (enemy.GetComponent<RevengeEffect>() is RevengeEffect revengeEffect)
        {
            Object.Destroy(revengeEffect);
            HeroController.instance.AddHealth(1 + (CombatRef.CombatLevel + CombatRef.EnduranceLevel) / 8);
        }
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
        On.HeroController.TakeDamage -= HeroController_TakeDamage;
        CombatRef.EnemyKilled -= CombatController_EnemyKilled;
    }
}
