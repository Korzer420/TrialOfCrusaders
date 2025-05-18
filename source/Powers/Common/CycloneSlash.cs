using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class CycloneSlash : Power
{
    public override string Name => "Cyclone Slash";

    public override string Description => "Unlocks Cyclone Slash";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable()
    {
        PDHelper.HasCyclone = true;
        PDHelper.HasNailArt = true;
    }

    internal override void Disable()
    {
        PDHelper.HasCyclone = false;
        PDHelper.HasNailArt = false;
    }
}
