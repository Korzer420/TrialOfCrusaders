using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class Regrets : Power
{
    public override DraftPool Pools => DraftPool.Instant | DraftPool.Risk;

    public override (float, float, float) BonusRates => new(0f, 0f, 0f);

    protected override void Enable() => PlayerData.instance.TakeGeo(PDHelper.Geo / 2);

    // Handled in treasure manager.
}
