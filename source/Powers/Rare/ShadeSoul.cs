using KorzUtils.Helper;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ShadeSoul : Power
{
    public override string Name => "Shade Soul";

    public override string Description => "Unlocks Shade Soul";

    public override (float, float, float) BonusRates => new(0f, 100f, 0f);

    public override Rarity Tier => Rarity.Rare;

    protected override void Enable()
    {
        PDHelper.FireballLevel = 2;
        PDHelper.HasSpell = true;
    }

    protected override void Disable()
    {
        PDHelper.FireballLevel = 0;
        PDHelper.HasSpell = false;
    }
}
