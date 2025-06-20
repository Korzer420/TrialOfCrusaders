using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Initiative : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override StatScaling Scaling => StatScaling.Combat;

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Burst;
}