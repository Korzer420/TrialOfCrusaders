using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class DreamNail : Power
{
    public override string Name => "Dream Nail";

    public override string Description => "Unlocks the dream nail to gather soul from hit foes.";

    public override (float, float, float) BonusRates => new(2f, 5f, 3f);

    internal override void Enable() => PDHelper.HasDreamNail = true;

    internal override void Disable() => PDHelper.HasDreamNail = false;
}
