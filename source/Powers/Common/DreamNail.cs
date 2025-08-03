using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class DreamNail : Power
{
    public override (float, float, float) BonusRates => new(2f, 5f, 3f);

    public override DraftPool Pools => DraftPool.Ability | DraftPool.Spirit;

    protected override void Enable() => PDHelper.HasDreamNail = true;

    protected override void Disable() => PDHelper.HasDreamNail = false;
}
