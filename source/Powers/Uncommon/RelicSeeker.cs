using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Uncommon;

public class RelicSeeker : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Wealth | DraftPool.Treasure;

    public override Rarity Tier => Rarity.Uncommon;
}
