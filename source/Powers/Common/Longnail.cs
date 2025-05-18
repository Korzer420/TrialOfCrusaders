using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Longnail : Power
{
    public override string Name => "Longnail";

    public override string Description => "Increases the range of your nail.";

    public override (float, float, float) BonusRates => new(7.5f, 0f, 2.5f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Longnail);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Longnail);
}
