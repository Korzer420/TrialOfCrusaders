using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class DescendingDark : Power
{
    public override string Name => "Descending Dark";

    public override string Description => "Unlocks Descending Dark";

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable()
    {
        PDHelper.QuakeLevel = 2;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.QuakeLevel = 0;
        PDHelper.HasSpell = false;
    }
}
