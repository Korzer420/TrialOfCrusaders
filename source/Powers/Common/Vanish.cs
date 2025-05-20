using System.Collections;
using System.Reflection;

namespace TrialOfCrusaders.Powers.Common;

internal class Vanish : Power
{
    private readonly static MethodInfo _invincibilityCall = typeof(HeroController).GetMethod("Invulnerable", BindingFlags.NonPublic | BindingFlags.Instance);

    public override (float, float, float) BonusRates => new(8f, 0f, 2f);

    protected override void Enable() => On.HealthManager.Die += HealthManager_Die;
    
    protected override void Disable() => On.HealthManager.Die -= HealthManager_Die;

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        HeroController.instance.StartCoroutine((IEnumerator)_invincibilityCall.Invoke(HeroController.instance, [1f]));
        orig(self, attackDirection, attackType, ignoreEvasion);
    }
}
