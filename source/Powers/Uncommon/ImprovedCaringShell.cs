using Modding;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedCaringShell : Power
{
    public override string Name => "Improved Caring Shell";

    public override string Description => "Decreases hazard damage even further. Hazards do not deal damage in calm rooms anymore.";

    public override (float, float, float) BonusRates => new(0f, 0f, 40f);

    public override Rarity Tier => Rarity.Uncommon;

    /// <inheritdoc/>
    protected override void Enable()
           => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;

    /// <inheritdoc/>
    protected override void Disable()
       => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;

    private int ModHooks_TakeDamageHook(int hazardType, int damageAmount)
    {
        if (hazardType > 1 && hazardType < 5 && damageAmount != 500)
            damageAmount -= 2 + (CombatController.EnduranceLevel / 4);
        return damageAmount;
    }
}
