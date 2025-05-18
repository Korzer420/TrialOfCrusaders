using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class HowlingWraiths : Power
{
    public override string Name => "Howling Wraiths";

    public override string Description => "Unlocks Howling Wraiths";

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    internal override void Enable()
    {
        PDHelper.ScreamLevel = 1;
        PDHelper.HasSpell = true;
    }

    internal override void Disable()
    {
        PDHelper.ScreamLevel = 0;
        PDHelper.HasSpell = false;
    }
}
