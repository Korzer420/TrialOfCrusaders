using TrialOfCrusaders.Data;

namespace TrialOfCrusaders.Powers.Common;

internal class Initiative : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);
}