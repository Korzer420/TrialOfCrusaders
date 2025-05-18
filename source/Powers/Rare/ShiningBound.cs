using Modding;
using System;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ShiningBound : Power
{
    public override string Name => "Shining Bound";

    public override string Description => "Halves all damage taken. You can no longer obtain charms.";

    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;

    internal override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;

    internal override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;

    private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
    {
        // 500 is instant kill.
        if (damageAmount != 0 && damageAmount != 500)
            damageAmount = (int)Math.Max(1, Math.Ceiling((float)damageAmount / 2));
        return damageAmount;
    }
}
