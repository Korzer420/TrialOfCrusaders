using KorzUtils.Enums;
using KorzUtils.Helper;

namespace TrialOfCrusaders.Powers.Common;

internal class Weaversong : Power
{
    public override string Name => "Weaversong";

    public override string Description => "Spawns weavers that assist you in battle";

    public override (float, float, float) BonusRates => new(7f, 0f, 3f);

    internal override void Enable() => CharmHelper.EnsureEquipCharm(CharmRef.Weaversong);

    internal override void Disable() => CharmHelper.UnequipCharm(CharmRef.Weaversong);
}
