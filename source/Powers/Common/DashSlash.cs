using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class DashSlash : Power
{
    public override string Name => "Dash Slash";

    public override string Description => "Unlocks Dash Slash";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        PDHelper.HasUpwardSlash = true;
        PDHelper.HasNailArt = true;
    }

    internal override void Disable()
    {
        PDHelper.HasUpwardSlash = false;
        PDHelper.HasNailArt = false;
    }
}
