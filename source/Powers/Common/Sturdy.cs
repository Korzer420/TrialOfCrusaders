using Modding;

namespace TrialOfCrusaders.Powers.Common;

internal class Sturdy : Power
{
    public override string Name => "Sturdy";

    public override string Description => "Decreases damage taken slightly.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;

    protected override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        if (damageAmount != 500 && damageAmount != 0)
            damageAmount -= 1 + CombatController.EnduranceLevel / 10;
        return damageAmount;
    }
}