using TrialOfCrusaders.Enums;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DeepCuts : Power
{
    public override (float, float, float) BonusRates => new(40f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;

    protected override void Disable() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (hitInstance.AttackType == AttackTypes.Nail && UnityEngine.Random.Range(0, 4) == 0)
            self.gameObject.AddComponent<BleedEffect>();
        orig(self, hitInstance);
    }
}
