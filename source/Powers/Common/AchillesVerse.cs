using Modding;
using System;

namespace TrialOfCrusaders.Powers.Common;

internal class AchillesVerse : Power
{
    public override string Name => "Achilles Verse";

    public override string Description => "Enemies deal less damage. Hazards now kill you instantly.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable() => ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;
    

    internal override void Disable() => ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;

    private int ModHooks_TakeDamageHook(int hazardType, int damageAmount)
    {
        if (hazardType > 1 && hazardType < 5)
            damageAmount = 500;
        else
            damageAmount = Math.Max(1, damageAmount - 2);
        return damageAmount;
    }
}