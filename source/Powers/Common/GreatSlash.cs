using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class GreatSlash : Power
{
    public override string Name => "Great Slash";

    public override string Description => "Unlocks Great Slash";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        PDHelper.HasDashSlash = true;
        PDHelper.HasNailArt = true;
    }

    internal override void Disable()
    {
        PDHelper.HasDashSlash = false;
        PDHelper.HasNailArt = false;
    }
}
