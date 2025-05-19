using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class DesolateDive : Power
{
    public override string Name => "Desolate Dive";

    public override string Description => "Unlocks Desolate Dive";

    public override (float, float, float) BonusRates => new(0f, 40f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    protected override void Enable()
    {
        PDHelper.QuakeLevel = 1;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.QuakeLevel = 0;
        PDHelper.HasSpell = false;
    }
}
