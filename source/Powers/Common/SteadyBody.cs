using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class SteadyBody : Power
{
    public override string Name => "Steady Body";

    public override string Description => "Removes the nail knockback";

    public override (float, float, float) BonusRates => new(10f, 0f, 0f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SteadyBody);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.SteadyBody);
}
