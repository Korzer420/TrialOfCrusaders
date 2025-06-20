using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class RoyalDecree : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override bool CanAppear => false;

    public override DraftPool Pools => DraftPool.Treasure;
}