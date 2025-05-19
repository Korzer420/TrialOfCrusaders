using Modding;

namespace TrialOfCrusaders.Powers.Common;

internal class CaringShell : Power
{
    public override string Name => "Caring Shell";

    public override string Description => "Hazards deal less damage.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    protected override void Enable()
    {
        ModHooks.AfterTakeDamageHook += ModHooks_TakeDamageHook;
    }

    protected override void Disable()
    {
        ModHooks.AfterTakeDamageHook -= ModHooks_TakeDamageHook;
    }

    private int ModHooks_TakeDamageHook(int hazardType, int damageAmount)
    {
        if (hazardType > 1 && hazardType < 5)
            damageAmount--;
        return damageAmount;
    }
}