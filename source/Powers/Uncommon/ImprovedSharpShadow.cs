using TrialOfCrusaders.Controller;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;
using TrialOfCrusaders.Powers.Rare;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedSharpShadow : Power
{
    public override bool CanAppear => !HasPower<InUtterDarkness>() && HasPower<SharpShadow>();

    public override (float, float, float) BonusRates => new(20f, 0f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    public override StatScaling Scaling => StatScaling.Combat | StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Upgrade | DraftPool.Ability | DraftPool.Combat | DraftPool.Endurance;

    protected override void Enable() => On.HealthManager.Die += HealthManager_Die;

    protected override void Disable() => On.HealthManager.Die -= HealthManager_Die;

    private void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
    {
        orig(self, attackDirection, attackType, ignoreEvasion);
        if (attackType == AttackTypes.SharpShadow)
            HeroController.instance.AddHealth(1 + (CombatRef.EnduranceLevel + (CombatRef.CombatLevel / 2)) / 4);
    }
}
