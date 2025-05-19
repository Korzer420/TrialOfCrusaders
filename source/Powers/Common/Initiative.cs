using System;
using TrialOfCrusaders.UnityComponents;

namespace TrialOfCrusaders.Powers.Common;

internal class Initiative : Power
{
    public override string Name => "Initiative";

    public override string Description => "The first nail hit on enemies deals bonus damage and grants extra soul.";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    protected override void Enable() => On.HealthManager.TakeDamage += HealthManager_TakeDamage;

    private void HealthManager_TakeDamage(On.HealthManager.orig_TakeDamage orig, HealthManager self, HitInstance hitInstance)
    {
        if (self.GetComponent<InitiativeEffect>() == null)
        {
            self.gameObject.AddComponent<InitiativeEffect>();
            
        }    
        orig(self, hitInstance);
    }

    protected override void Disable() => On.HealthManager.TakeDamage -= HealthManager_TakeDamage;
}