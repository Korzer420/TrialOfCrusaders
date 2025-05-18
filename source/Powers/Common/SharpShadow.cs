using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class SharpShadow : Power
{
    public override string Name => "Sharp Shadow";

    public override string Description => "Your shade dash is extended and deals damage.";

    public override (float, float, float) BonusRates => new(5f, 0f, 5f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.SharpShadow);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.SharpShadow);
}
