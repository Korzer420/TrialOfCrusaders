using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class DashSlash : Power
{
    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    public override bool CanAppear => PDHelper.HasDash;

    protected override void Enable()
    {
        PDHelper.HasUpwardSlash = true;
        PDHelper.HasNailArt = true;
    }

    protected override void Disable()
    {
        PDHelper.HasUpwardSlash = false;
        PDHelper.HasNailArt = false;
    }
}
