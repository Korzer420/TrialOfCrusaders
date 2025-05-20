using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Rare;

internal class ShiningBound : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 100f);

    public override Rarity Tier => Rarity.Rare;
}
