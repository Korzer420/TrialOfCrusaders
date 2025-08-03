using KorzUtils.Helper;
using TrialOfCrusaders.Data;
using TrialOfCrusaders.Enums;

namespace TrialOfCrusaders.Powers.Common;

internal class CycloneSlash : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override DraftPool Pools => DraftPool.Combat | DraftPool.Ability;

    protected override void Enable()
    {
        PDHelper.HasCyclone = true;
        PDHelper.HasNailArt = true;
    }

    protected override void Disable()
    {
        PDHelper.HasCyclone = false;
        PDHelper.HasNailArt = false;
    }
}
