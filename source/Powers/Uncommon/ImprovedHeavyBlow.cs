using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.UnityComponents.Debuffs;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedHeavyBlow : Power
{
    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override bool CanAppear => HasPower<HeavyBlow>();

    protected override void Enable() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    
    protected override void Disable() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
        {
            bool buffedHit = UnityEngine.Random.Range(1, 21) == 1;
            if (self.GetComponent<ConcussionEffect>() is ConcussionEffect concussionComponent)
                concussionComponent.Timer += buffedHit ? 3 : 0.5f;
            else if (buffedHit)
                self.gameObject.AddComponent<ConcussionEffect>();
        }
        orig(self, hitInstance);
    }
}