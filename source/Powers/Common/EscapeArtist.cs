using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class EscapeArtist : Power
{
    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    public override StatScaling Scaling => StatScaling.Endurance;

    public override DraftPool Pools => DraftPool.Endurance;
}