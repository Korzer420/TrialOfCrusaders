using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class BrighterFuture : Power
{
    public override Rarity Tier => Rarity.Uncommon;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Wealth | DraftPool.Burst;
}
