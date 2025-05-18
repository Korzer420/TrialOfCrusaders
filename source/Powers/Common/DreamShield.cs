using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Dreamshield : Power
{
    public override string Name => "Dream Shield";

    public override string Description => "Spawns a rotating shield around you.";

    public override (float, float, float) BonusRates => new(0f, 0f, 10f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.DreamShield);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.DreamShield);
}
