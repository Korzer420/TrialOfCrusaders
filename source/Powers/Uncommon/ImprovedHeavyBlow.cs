using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;
using UnityEngine;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedHeavyBlow : Power
{
    public override string Name => "Improved Heavy Blow";

    public override string Description => "Adds a chance to cause a concussion on enemies.";

    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;
    
    internal override void Disable() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail)
        {
            
            bool buffedHit = UnityEngine.Random.Range(1, 26) == 1;
            if (self.GetComponent<ConcussionEffect>() is ConcussionEffect concussionComponent)
                concussionComponent.ConcussiveTime += buffedHit ? 3 : 0.5f;
            else
            {
                GameObject child = GameObject.Instantiate(ConcussionEffect.ConcussionObject, self.transform);
                child.name = "Concussion";
                child.AddComponent<ConcussionEffect>();
                // This should place the concussive sprite on top of the enemy.
                child.transform.localPosition = new(0, 0 + self.transform.GetComponent<BoxCollider2D>().size.y / 2);
            }
        }
        orig(self, hitInstance);
    }
}