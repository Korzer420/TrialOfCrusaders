using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class Discount : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Wealth;
}
