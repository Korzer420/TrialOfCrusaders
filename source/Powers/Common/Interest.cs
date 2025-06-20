using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Interest : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Wealth;
}
