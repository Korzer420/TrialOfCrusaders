using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;
using TrialOfCrusaders.Powers.Common;

namespace TrialOfCrusaders.Powers.Uncommon;

internal class ImprovedSprintmaster : Power
{
    public override (float, float, float) BonusRates => new(20f, 0f, 20f);

    public override Rarity Tier => Rarity.Uncommon;

    public override DraftPool Pools => DraftPool.Charm | DraftPool.Upgrade;

    public override bool CanAppear => HasPower<Sprintmaster>();
}
